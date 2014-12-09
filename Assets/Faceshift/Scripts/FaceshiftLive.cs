/*
 *  Copyright 2013 faceshift AG. All rights reserved.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;

namespace fs
{
		public class FaceshiftLive : MonoBehaviour, NetworkConnectionCallback
		{
	
				// public variables to access network
				public string hostName = "127.0.0.1";
				public int port = 33433;
				public TextAsset RetargetingAsset = null;
				public TextAsset TPoseAsset = null;
	
				// Init variables ----------------------------
				private Mutex m_mutex;
				private NetworkConnection m_connection;
				private DataParser m_data_parser;
				private TrackData m_current_track;
				private Rig m_rig;
				private ClipRetargeting m_retargeting;
				private TPose m_tpose;

				//! Caches a list of possible target blend shapes (in the connected game object)
				public ArrayList m_game_object_blendshapes = new ArrayList ();
	
				//! Caches a list of possible target transfromations (in the connected game object)
				public ArrayList m_game_object_transformations = new ArrayList ();

				// Loops -------------------------------------
				void Awake ()
				{
		
				}

				void Start ()
				{
						m_mutex = new Mutex ();
		
						m_data_parser = new DataParser ();
						m_current_track = new TrackData ();
		
						m_connection = new NetworkConnection (this);
						if (!m_connection.Start (hostName, port, true)) {
								Debug.LogError ("could not access faceshift over the network on " + hostName + ":" + port + " using the TCP/IP protocol");
						}

						// get the blendshapes from fs studio
						askForBlendshapeNames ();

						if (RetargetingAsset != null) {
								m_retargeting = ClipRetargeting.load (RetargetingAsset.bytes);
								if (m_retargeting == null) {
										Debug.LogError ("could not load retargeting from asset");
								}
						}
		
						if (TPoseAsset != null) {
								m_tpose = new TPose ();
								if (!m_tpose.loadFromBytes (TPoseAsset.bytes)) {
										Debug.LogError ("could not load tpose from asset");
								}
						}

						// get game object blendshapes and transformations
						Utils.getGameObjectBlendshapes (gameObject, m_game_object_blendshapes);
						Utils.getGameObjectTransformations (gameObject, m_game_object_transformations);
				}

				void Update ()
				{	
						/*for (int index = 0; index < m_current_track.MarkersPositions.Count; index++) {
				Debug.LogError (" index " + m_current_track.MarkersPositions[index]);
			}*/
						//for (int index = 0; index < m_current_track.Coefficient.Length; index++) {
						//Debug.LogError (" L " + m_current_track.Coefficient[30]);
						//Debug.LogError (" R " + m_current_track.Coefficient[31]);
			//Debug.LogError ("cos " + getXHeadTranslation ());
						//}
						/*for (int index = 0; index < m_data_parser.fsBlendShapeNames.Length; index++) {
				Debug.LogError ("HeadRotation " + m_data_parser.fsBlendShapeNames[index]);//m_current_track.HeadRotation().x);
			}
			for (int index = 0; index < m_game_object_blendshapes.Count; index++) {
				Debug.LogError ("m_game_object_blendshapes " + m_game_object_blendshapes[index]); 
			}
			Debug.LogError(" HeadTranslation x " + m_current_track.HeadRotation().x);
			Debug.LogError(" HeadTranslation y " + m_current_track.HeadRotation().y);
			Debug.LogError(" HeadTranslation z " + m_current_track.HeadRotation().z);*/
						//Debug.LogError(" HeadTranslation y " + m_current_track.HeadRotation().y);
						//m_current_track.

						bool new_data = false;
						m_mutex.WaitOne ();
						// check if there are new blendshape names, and if so update
						if (newBlendShapeNamesArrived ()) {
								m_rig = getRigFromBlendShapeNames (getBlendShapeNames ());
						}

						// get most recent tracking data
						while (m_data_parser.CountAvailableTracks() > 0) {
								m_current_track = m_data_parser.Dequeue ();
								new_data = true;
						}

						// check that we have a rig (set up from blendshape names) with the same number of blendshapes as what we receive
						if (m_rig != null && m_current_track != null) {
								int n_track_coefficients = m_current_track.n_coefficients ();
								int n_rig_coefficients = m_rig.num_shapes ();
								if (n_track_coefficients != n_rig_coefficients) {
										Debug.LogWarning ("number of coefficients of rig and tracking state have changed: " + n_track_coefficients + " vs " + n_rig_coefficients);
										// clear the current rig as it is not usable with the data coming from faceshift
										m_rig = null;
										m_current_track = null;
										// get again the blendshapes from fs studio
										askForBlendshapeNames ();
								}
						}
						m_mutex.ReleaseMutex ();
			
						if (m_rig != null && m_current_track != null && m_retargeting != null) {
								if (new_data && m_current_track.TrackSuccess) {
										// create a rig state
										RigState state = new RigState (m_rig);
										state.set_timestamp (m_current_track.TimeStamp);
										state.set_bone_translation (0, m_current_track.HeadTranslation ());
										state.set_bone_rotation (0, m_current_track.HeadRotation ());
										state.set_bone_rotation (1, m_current_track.LeftEyeRotation ());
										state.set_bone_rotation (2, m_current_track.RightEyeRotation ());
										for (int i = 0; i < m_rig.num_shapes(); i++) {
												state.set_blendshape_coefficient (i, m_current_track.Coefficient [i]);
										}
				
										// evaluate joint transformations
										TransformationValue [] transformation_values = null;
										if (m_tpose != null && m_tpose.m_joints.Count == m_game_object_transformations.Count) {
												// evaluate using tpose
												transformation_values = Utils.evaluate_target_transformations (m_retargeting, m_rig, state, m_tpose.m_joints);
										} else {
												// evaluate using state from start of application
												transformation_values = Utils.evaluate_target_transformations (m_retargeting, m_rig, state, m_game_object_transformations);
										}
										Debug.LogError ("transformation_values " + transformation_values);
										if (transformation_values.Length == m_game_object_transformations.Count) {
												for (int index = 0; index < transformation_values.Length; index++) {
														// Apply the value for this target
														if (transformation_values [index] != null) {
																TransformationInformation joint = m_game_object_transformations [index] as TransformationInformation;
																joint.transform.localRotation = transformation_values [index].m_rotation;
																joint.transform.localPosition = transformation_values [index].m_translation;
														}
												}
										} else {
												Debug.LogError ("Cannot create transformation as evaluated shape size is incorrect");
										}
		
										// evaluate blendshape valuesf
										BlendshapeValue [] values = Utils.evaluate_target_blendshapes (m_retargeting, m_rig, state, m_game_object_blendshapes);

										if (values.Length == m_game_object_blendshapes.Count) {
												for (int index = 0; index < m_game_object_blendshapes.Count; index++) {
														BlendshapeInfo bs_info = m_game_object_blendshapes [index] as BlendshapeInfo;
														// Apply the value for this target
														if (bs_info != null && values [index] != null) {
																bs_info.m_mesh_renderer.SetBlendShapeWeight (bs_info.m_index, (float)values [index].m_value);
														}
												}
										} else {
												Debug.LogError ("Cannot create blendshapes as evaluated shape size is incorrect");
										}
								}
						}
				}
	
				void OnDestroy ()
				{
						if (m_connection != null)
								m_connection.Stop ();
						m_connection = null;
				}

	
				// Callback when new data arrives from faceshift studio
				public void OnNewData (byte[] data)
				{
						m_mutex.WaitOne ();
						m_data_parser.ExtractData (data);
						m_mutex.ReleaseMutex ();
				}
	
				//! Asks faceshift studio to send the current blendshape names
				private bool askForBlendshapeNames ()
				{
						m_mutex.WaitOne ();
						// send command to faceshift studio to get the blendshape names
						bool result = m_connection.SendCommand (44744);
						m_mutex.ReleaseMutex ();
						return result;
				}
	
				//! @returns True if new blendshape names have arrived.
				private bool newBlendShapeNamesArrived ()
				{
						return m_data_parser.newBlendShapeNames;
				}
	
				//! @returns the List of blendshape names that are streamed from faceshift studio.
				private string[] getBlendShapeNames ()
				{
						m_mutex.WaitOne ();
						string [] targetArray = null;
						if (m_data_parser.fsBlendShapeNames != null) {
								targetArray = new string[m_data_parser.fsBlendShapeNames.Length];
								m_data_parser.fsBlendShapeNames.CopyTo (targetArray, 0);
						}
						m_data_parser.newBlendShapeNames = false;
						m_mutex.ReleaseMutex ();
						return targetArray;
				}

				//! @returns a rig build from a set of blendshape names
				private Rig getRigFromBlendShapeNames (string[] blendshape_names)
				{
						Rig rig = new Rig ();
						rig.add_bone ("Neck");
						rig.add_bone ("EyeLeft");
						rig.add_bone ("EyeRight");
						for (int i = 0; i < blendshape_names.Length; i++) {
								rig.add_shape (blendshape_names [i]);
						}
						return rig;
				}

				public float getXHeadTranslation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadTranslation ().x;
						} else {
								return -1;
						}
				}
		
				public float getYHeadTranslation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadTranslation ().y;
						} else {
								return -1;
						}
				}
				//
				public float getZHeadTranslation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadTranslation ().z;
						} else {
								return -1;
						}
				}
		
		//Głowa -> Gora-dół
		public float getYHeadRotation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadRotation ().x;
						} else {
								return -1;
						}
				}
		
		//Głowa -> Lefo-prawo
				public float getXHeadRotation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadRotation ().y;
						} else {
								return -1;
						}
				}
				public float getZHeadRotation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadRotation ().z;
						} else {
								return -1;
						}
				}
				//Głowa -> zblizenie do barkow
				public float getWHeadRotation ()
				{
						if (m_current_track != null) {
								return m_current_track.HeadRotation ().w;
						} else {
								return -1;
						}
				}

				public float getHappiness ()
				{
						if (m_current_track != null) {
								return (m_current_track.Coefficient [31] + m_current_track.Coefficient [30]) / 2;
						} else {
								return -1;
						}
				}
		}
	
}