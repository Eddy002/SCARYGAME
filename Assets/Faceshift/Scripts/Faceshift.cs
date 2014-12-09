/*
 *  Copyright 2013 faceshift AG. All rights reserved.
 */
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Threading;
using System.IO;

namespace fs
{
/**
 * @brief This is the main class used for the faceshift retargeting. Simply drag this script onto your game object.
 */
public class Faceshift : MonoBehaviour
{
    //! Caches a list of possible target blend shapes (in the connected game object)
    public ArrayList targetBlendShapeInfos = null;
    
    //! Caches a list of possible target transformations (in the connected game object)
    public ArrayList targetTransformations = null;

    //! The source rig
	public Rig m_fs_Rig = null;

    //! The currently defined retargeting (mapping)
    public ClipRetargeting m_retargeting = null;

	//! Asset for retargeting
	public TextAsset m_retargeting_asset = null;
	
    //! To make sure only one thread accesses the list at a time
    private Mutex mutexOnTargetBlendshapeList = null;

    //! The animation clip (source) we want to retarget
    public Clip m_clip = null;

	//! The animation clip path.
	public string m_clip_path = "";

    //! To make sure we do not warn the user of a changed rig if there was no rig before
	private bool cachedRigLoaded = false;

    //! Set to true, if the message popup with the first start information should be shown
    private bool showFirstStartMessage = false;

	//! To apply the transformation of an animation, we need to know the tpose transformations.
	//! If the tpose is null, then the tpose is the initial pose of the game object.
	public TPose m_tpose = null;

	//! asset for tpose
	public TextAsset m_tpose_asset = null;


    public Faceshift()
    {
		mutexOnTargetBlendshapeList = new Mutex();
        m_retargeting = new ClipRetargeting();
		targetBlendShapeInfos = new ArrayList();
        targetTransformations = new ArrayList();
        m_fs_Rig = new Rig();
        cachedRigLoaded = LoadCachedSourceRig();
			 
        if (!cachedRigLoaded) {
            // There was no rig cached. So we assume this was the first start
            showFirstStartMessage = true;
        }
    }


    /**
     * @brief Displays a popup message at the first start to inform the user how she/he can get clips
     */
    public void showPopupMessageIfFirstStart()
    {
        if (showFirstStartMessage) {
            showFirstStartMessage = false;
				 
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("Information", "If you don't have faceshift yet, get the trial at www.faceshift.com", "Get the Trial", "OK")) {
                // Open URL
                Application.OpenURL("http://www.faceshift.com/get-trial/");
            }
#endif
        }
    }

	//! Init function initializing the retargeting, clip, and tpose (can be called any amount of times)
	public void Init() {  
		// check for clips
		if (m_clip_path != "" && m_clip == null) {
			LoadFSB(m_clip_path);
			// no clip, so the clip file is invalid
			if (m_clip == null) m_clip_path = "";
		}

		// check for retarging (use asset if available, otherwise path)
		if (m_retargeting_asset != null && m_retargeting.isEmpty()) {
			if (!LoadRetargetingFromAsset(m_retargeting_asset)) {
				m_retargeting_asset = null;
			}
		}
		
		// check for tpose (use asset if available, otherwise path)
		if (m_tpose_asset != null && m_tpose == null) {
			LoadTPoseFromAsset(m_tpose_asset);
			// no tpose, so the tpose asset is invalid
			if (m_tpose == null) m_tpose_asset = null;
		}
	}

	public void ClearRetargeting() {
		m_retargeting = new ClipRetargeting ();
		m_retargeting_asset = null;
	}

    /**
     * @brief Loads an FST file from faceshift which contains the retargeting information
     * @param[in] path The path to the file
     * @param onlyRig Should only the rig be loaded (true) or also the mapping (false)
     */
    public bool LoadRetargetingFromFile(string path)
    {
        Rig new_rig = new Rig();
		if (!new_rig.load_from_fst(path)) return false;

        if (!new_rig.Equals(m_fs_Rig)) {
            if ((cachedRigLoaded) && (!source_rig_changed_ask_to_continue())) {
                // The user wants to cancel
                return false;
            }

            // Apply new rig
            m_fs_Rig = new_rig;

            // Cache new rig
            SaveSourceRig();
        }

    	ClipRetargeting retargeting = ClipRetargeting.load(path);
		if (retargeting != null && !retargeting.isEmpty()) {
			m_retargeting = retargeting;
			m_retargeting_asset = null; // clear asset as we use a file
			return true;
		} else {
			return false;
		}   
    }

	/**
     * @brief Loads an FST retargeting file from faceshift which contains the retargeting information
     * @param[in] path The path to the file
     * @param onlyRig Should only the rig be loaded (true) or also the mapping (false)
     */
	public bool LoadRetargetingFromAsset(TextAsset asset)
	{
		Rig new_rig = new Rig();
		if (!new_rig.load_from_fst(asset.bytes)) return false;
		
		if (!new_rig.Equals(m_fs_Rig)) {
			if ((cachedRigLoaded) && (!source_rig_changed_ask_to_continue())) {
				// The user wants to cancel
				return false;
			}
			
			// Apply new rig
			m_fs_Rig = new_rig;
			
			// Cache new rig
			SaveSourceRig();
		}
		
		ClipRetargeting retargeting = ClipRetargeting.load(asset.bytes);
		if (retargeting != null && !retargeting.isEmpty()) {
			m_retargeting = retargeting;
			m_retargeting_asset = asset; 
			return true;
		} else {
			return false;
		}   
	}

    //! @brief Saves the retargeting (including the source rig) into a FST file
    public void SaveRetargetingToFile(string path)
    {
        if (m_retargeting.save (path, m_fs_Rig)) {
			m_retargeting_asset = null; // clear asset as we use file
		}
    }

#if UNITY_EDITOR
	public void SaveRetargetingToAsset(string path) 
	{
		if (m_retargeting.save(path, m_fs_Rig)) {
			AssetDatabase.Refresh();

			string asset_path = Utils.AssetPath(path);
			Debug.Log ("loading asset " + asset_path);
			m_retargeting_asset = AssetDatabase.LoadAssetAtPath(asset_path, typeof(TextAsset)) as TextAsset;
			if (m_retargeting_asset == null) {
				Debug.LogError ("no retargeting asset at path " + asset_path);
			}
		}
	}
#endif

	public void CleanupRetargeting() {
		int index = 0;
		while (index < m_retargeting.get_number_of_blendshape_mappings()) {
			if (m_fs_Rig.shape_index(m_retargeting.get_blendshape_mapping_source(index)) < 0 ||
				    !has_target_blendshape_name(m_retargeting.get_blendshape_mapping_destination(index))) {
					m_retargeting.remove_blendshape_mapping(index);
			} else index++;
		}
		index = 0;
		while (index < m_retargeting.get_number_of_rotation_mappings()) {
			if (m_fs_Rig.bone_index(m_retargeting.get_rotation_mapping_source(index)) < 0 ||
			    !has_target_bone_name(m_retargeting.get_rotation_mapping_destination(index))) {
				m_retargeting.remove_rotation_mapping(index);
			} else index++;
		}
		index = 0;
		while (index < m_retargeting.get_number_of_translation_mappings()) {
			if (m_fs_Rig.bone_index(m_retargeting.get_translation_mapping_source(index)) < 0 ||
			    !has_target_bone_name(m_retargeting.get_translation_mapping_destination(index))) {
				m_retargeting.remove_translation_mapping(index);
			} else index++;
		}
	}
	

    //! @brief Saves (caches) the faceshift rig (source rig)
    public void SaveSourceRig()
    {
        m_fs_Rig.save_to_fst("Cached_fs_rig.fst");
        cachedRigLoaded = true;
    }

    //! @brief Loads the faceshift rig (source rig) from the cache file
    public bool LoadCachedSourceRig()
    {
        return m_fs_Rig.load_from_fst("Cached_fs_rig.fst");
    }

    //! @brief Loads a faceshift clip
    public void LoadFSB(string path)
    {
		if (!File.Exists (path)) {
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog("Clip loading failed", "File " + path + " does exist", "Ok");
			#endif
			return;
		}
        Clip new_clip = FsbReader.read(path);
		if (new_clip == null) {
			Debug.LogError("could not read clip from file " + path);
			#if UNITY_EDITOR
			EditorUtility.DisplayDialog("Load failed", "File " + path + " does not contain a clip", "Ok");
			#endif
			return;
		}

        Rig clip_rig = new_clip.rig();

        if (!clip_rig.Equals(m_fs_Rig)) {
            if ((cachedRigLoaded) && (!source_rig_changed_ask_to_continue())) {
                // The user wants to cancel
                return;
            }

            // Apply new rig
            m_fs_Rig = clip_rig;

            // Cache new rig
            SaveSourceRig();
        }

        m_clip = new_clip;
		m_clip_path = path;
    }

    /**
     * @brief Shows a message popup to the user stating that the source rig changed and asks the user to continue
     * @return true, if the user wants to continue and false otherwise
     */
    public bool source_rig_changed_ask_to_continue()
    {
#if UNITY_EDITOR
        return (EditorUtility.DisplayDialog("Different source rig!",
                                            "The file contains a different source rig. Do you want to continue?", "Yes", "No"));
#else
        return true;
#endif
    }

	//! Clears the tpose
	public void ClearTPose() {
		m_tpose = null;
		m_tpose_asset = null;
	}

	/**
	 * Saves the current pose of the character as T-pose
	 * @param path [in] The path to the file
	 */
	public void SetTPose() {
		m_tpose = new TPose();
		Utils.getGameObjectTransformations (gameObject, m_tpose.m_joints);
	}

	//! Loads the t pose of the character.
	public bool LoadTPoseFromFile(string filename_with_path) {
		TPose new_tpose = new TPose();
		if (new_tpose.loadFromFile (filename_with_path)) {
			m_tpose = new_tpose;
			m_tpose_asset = null;
			return true;
		} else {
			return false;
		}
	}

	//! Loads the t pose of the character.
	public bool LoadTPoseFromAsset(TextAsset asset) {
		TPose new_tpose = new TPose();
		if (new_tpose.loadFromBytes(asset.bytes)) {
			m_tpose = new_tpose;
			m_tpose_asset = asset;
			return true;
		} else {
			return false;
		}
	}

	/**
	 * Saves the current pose of the character as T-pose
	 * @param path [in] The path to the file
	 */
	public void SaveTPoseToFile(string filename_with_path) {
		if (m_tpose.saveToFile (filename_with_path)) {
			m_tpose_asset = null;
		}
	}

#if UNITY_EDITOR
	public void SaveTPoseToAsset(string path) 
	{
			if (m_tpose.saveToFile(path)) {
			AssetDatabase.Refresh();
			// convert to asset path
			string asset_path = "Assets/" + path.Remove(0, Application.dataPath.Length+1);
			m_tpose_asset = AssetDatabase.LoadAssetAtPath(asset_path, typeof(TextAsset)) as TextAsset;
			if (m_tpose_asset == null) {
				Debug.LogError ("no tpose asset at path " + asset_path);
			}
		}
	}
#endif

		/**
     * @brief Stores the current clip into a Unity .anim file
     * Important: If you use your own character with blend shapes, you have to make sure it is
     * set to 'legacy' animation type. You can do this by the following steps in Unity3D:
     * 1. In the 'project' window in the Assets hierarchy, click on your fbx model
     * 2. In the Inspector, you should see now the Import Settings of your model.
     * 3. Select in these Import Settings in the 'Rig' tab for the 'Animation Type' the value 'Legacy'
     */ 
		public void SaveClipAsAnim(string path)
		{  
			bool have_t_pose = false; 
			if (m_tpose != null && m_tpose.m_joints != null) {
				if (m_tpose.m_joints.Count != targetTransformations.Count) {
					Debug.LogError ("tpose and model do not have the same number of transformations (" + m_tpose.m_joints.Count + "!=" 
					                + targetTransformations.Count + ")");
					return;
				}
				have_t_pose = true;
			} else {
				Debug.LogWarning ("tpose missing");
			}


			AnimationClip animation_clip = new AnimationClip();
			
			// Not all of them might actually be used, since not all of them might be a target
			AnimationCurve [] translation_x_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] translation_y_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] translation_z_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] rotation_x_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] rotation_y_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] rotation_z_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] rotation_w_curves = new AnimationCurve[targetTransformations.Count];
			AnimationCurve [] bs_curves = new AnimationCurve[targetBlendShapeInfos.Count];
			
			Debug.Log("nr of clip states: " + m_clip.num_states());
			for (int frame_nr = 0; frame_nr < m_clip.num_states(); frame_nr++) {
				// get frame time
				float time = (float)(m_clip[frame_nr].timestamp() - m_clip[0].timestamp()) * 0.001f; // time is in ms

				// evaluate transformation
				TransformationValue [] transformation_values = null;
				if (have_t_pose) {
					transformation_values = Utils.evaluate_target_transformations(m_retargeting, m_fs_Rig, m_clip[frame_nr], m_tpose.m_joints);
				} else {
					transformation_values = Utils.evaluate_target_transformations(m_retargeting, m_fs_Rig, m_clip[frame_nr], targetTransformations);
				}

				if (transformation_values.Length == targetTransformations.Count) {
					for (int index = 0; index < transformation_values.Length; index++) {
						// Apply the value for this target
						if (transformation_values[index] != null) {
							// Apply the translation value for this target
							if (translation_x_curves[index] == null) {
								translation_x_curves[index] = new AnimationCurve();
								translation_y_curves[index] = new AnimationCurve();
								translation_z_curves[index] = new AnimationCurve();
							}
							translation_x_curves[index].AddKey(time, transformation_values[index].m_translation.x);
							translation_y_curves[index].AddKey(time, transformation_values[index].m_translation.y);
							translation_z_curves[index].AddKey(time, transformation_values[index].m_translation.z);

							if (rotation_x_curves[index] == null) {
								rotation_x_curves[index] = new AnimationCurve();
								rotation_y_curves[index] = new AnimationCurve();
								rotation_z_curves[index] = new AnimationCurve();
								rotation_w_curves[index] = new AnimationCurve();
							}
							// Add to curve for the animation
							rotation_x_curves[index].AddKey(time, transformation_values[index].m_rotation.x);
							rotation_y_curves[index].AddKey(time, transformation_values[index].m_rotation.y);
							rotation_z_curves[index].AddKey(time, transformation_values[index].m_rotation.z);
							rotation_w_curves[index].AddKey(time, transformation_values[index].m_rotation.w);
						}
					}
				} else {
					Debug.LogError("Cannot create transformation as evaluated shape size is incorrect");
				}

				
				// evaluate blendshapes
				BlendshapeValue [] blendshape_values = Utils.evaluate_target_blendshapes(m_retargeting, m_clip.rig (), m_clip[frame_nr], targetBlendShapeInfos);
				if (blendshape_values.Length == targetBlendShapeInfos.Count) {
					for (int index = 0; index < targetBlendShapeInfos.Count; index++) {
						// Apply the value for this target
						if (blendshape_values[index] != null) {
							if (bs_curves[index] == null) {
								bs_curves[index] = new AnimationCurve();
							}
							bs_curves[index].AddKey(time, (float)blendshape_values[index].m_value);
						}
					}
				} else {
					Debug.LogError("Cannot create blendshapes as evaluated shape size is incorrect");
				}
			}
			
			// Set all transformation curves for all transformation that are animated
			for (int target_nr = 0; target_nr < targetTransformations.Count; target_nr++) {
				// Extract path:
				string path_to_transformation = ((TransformationInformation)targetTransformations[target_nr]).transformPath;
				// Apply translation curve, if there is one
				if (translation_x_curves[target_nr] != null) {
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localPosition.x", translation_x_curves[target_nr]);
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localPosition.y", translation_y_curves[target_nr]);
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localPosition.z", translation_z_curves[target_nr]);
				}
				// Apply rotation curve, if there is one
				if (rotation_x_curves[target_nr] != null) {
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localRotation.x", rotation_x_curves[target_nr]);
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localRotation.y", rotation_y_curves[target_nr]);
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localRotation.z", rotation_z_curves[target_nr]);
					animation_clip.SetCurve(path_to_transformation, typeof(Transform), "localRotation.w", rotation_w_curves[target_nr]);
				}
			}

			// Without this, there are some weird jumps (rotation) in the animation:
			animation_clip.EnsureQuaternionContinuity();
			
			// Set all blendshape curves for all blendshapes that are animated
			for (int i = 0; i < targetBlendShapeInfos.Count; i++) {
				if (bs_curves[i] != null) {
					BlendshapeInfo bs_info = (BlendshapeInfo)(targetBlendShapeInfos[i]);
					// Debug.Log("bs_curves[" + i + "].length=" + bs_curves[i].length);
					string bs_path = bs_info.m_path;
					string bs_name = bs_info.m_name;
					animation_clip.SetCurve(bs_path, typeof(SkinnedMeshRenderer), "blendShape." + bs_name, bs_curves[i]);
				}
			}
			
			// animation clip asset 
			string animation_name = Path.GetFileNameWithoutExtension(path);
			animation_clip.name = animation_name;
			#if UNITY_EDITOR
			AssetDatabase.CreateAsset(animation_clip, path);
			#endif
			 
			// Add the clip to the animations of the attached game object
			Animation my_animation = gameObject.animation;
			if (!my_animation) {
				gameObject.AddComponent(typeof(Animation));
				my_animation = gameObject.animation;
			}
			if (my_animation) {
				my_animation.AddClip(animation_clip, animation_name);
				my_animation.clip = animation_clip;
				my_animation[animation_name].blendMode = AnimationBlendMode.Blend;
			} else {
				print("Error: could not get animation object");
			}
			Debug.Log("Wrote animation with length " + (1000.0 * animation_clip.length) + " milliseconds");

		}

		
	/**
     * Parses the connected game object and all sub objects for
     * blendshapes and tranformation. Writes these into targetBlendShapes
     * and targetTransformations, respectively.
     **/
	public void update_list_of_possible_target_blendshapes_and_tranformation()
	{
		mutexOnTargetBlendshapeList.WaitOne();
		// Clear the cache
		targetBlendShapeInfos.Clear();
		targetTransformations.Clear();
		// get the transformations
		Utils.getGameObjectBlendshapes (gameObject, targetBlendShapeInfos);
		Utils.getGameObjectTransformations (gameObject, targetTransformations);
		mutexOnTargetBlendshapeList.ReleaseMutex();
	}

	
	void Awake() {
	}

    void Start ()
    {
    }

    void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update ()
    {
    }


	//! @returns all target blendshape names
	public string [] target_blendshape_names() {
		string [] names = new string[targetBlendShapeInfos.Count];
		for (int i = 0; i < targetBlendShapeInfos.Count; i++) {
			names[i] = ((BlendshapeInfo)(targetBlendShapeInfos[i])).m_name;
		}
		return names;
	}

	//! @returns True if the target contains the blendshape name.
	bool has_target_blendshape_name(string name) {
		for (int i = 0; i < targetBlendShapeInfos.Count; i++) {
			if (((BlendshapeInfo)(targetBlendShapeInfos[i])).m_name == name) return true;
		}
		return false;
	}

	//! @returns all target transformation names
	public string [] target_transformation_names() {
		string [] names = new string[targetTransformations.Count];
		for (int i = 0; i < targetTransformations.Count; i++) {
			names[i] = ((TransformationInformation)(targetTransformations[i])).transformName;
		}
		return names;
	}

	//! @returns True if the target contains the bone name.
	bool has_target_bone_name(string name) {
		for (int i = 0; i < targetTransformations.Count; i++) {
			if (((TransformationInformation)(targetTransformations[i])).transformName == name) return true;
		}
		return false;
	}

}

}
