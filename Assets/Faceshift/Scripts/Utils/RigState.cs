/*
 *  Copyright 2013 faceshift AG. All rights reserved.
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace fs
{
/**
 * @brief class RigState contains the pose, timestamp, blendshape coefficients and bone rotations of a Rig.
 */
public class RigState
{
    //! timestamp in ms
    private double m_timestamp;

    //! blendshape values (from 0 to 1)
    private double[] m_blendshape_coefficients;

    //! bone values (in left hand coordinate system)
    private Vector3[] m_bone_translations;

    //! bone values (in left hand coordinate system)
    private Quaternion[] m_bone_rotations;

    /**
     * @brief Create track state with shapes and bones.
     * @param[in] rig     The rig for which to create the state.
     */
    public RigState(Rig rig)
    {
        m_blendshape_coefficients = new double[rig.num_shapes()];
        m_bone_translations = new Vector3[rig.num_bones()];
        m_bone_rotations = new Quaternion[rig.num_bones()];
    }

    /**
     * @brief Get the timestamp of this state.
     * @Return The timestamp value of this state.
     */
    public double timestamp() { return m_timestamp; }

    /**
     * @brief Set the timestamp of this state.
     * @param[in] value    The new timestamp value.
     */
    public void set_timestamp(double value) { m_timestamp = value; }

    /**
     * @brief Get the blendshape coefficient.
     * @param[in] index   Blendshape coefficient index (no check for validity of index).
     * @return Blendshape coefficient at index.
     */
    public double blendshape_coefficient(int index) { return m_blendshape_coefficients[index]; }

    /**
     * @brief Set the blendshape coefficient.
     * @param[in] index       Blendshape coefficient index (no check for validity of index).
     * @param[in] value    New blendshape coefficient value.
     */
    public void set_blendshape_coefficient(int index, double value) { m_blendshape_coefficients[index] = value; }

    /**
     * @brief Get the translation of a bone.
     * @param[in] index   Bone coefficient index (no check for validity of index).
     */
    public Vector3 bone_translation(int index) { return m_bone_translations[index]; }

    /**
     * @brief Set the translation of a bone.
     * @param[in] index   Bone index (no check for validity of index).
     * @param[in] value   New bone translation vector.
     */
    public void set_bone_translation(int index, Vector3 value) { m_bone_translations[index] = value; }

    /**
     * @brief Get the quaternion of a bone.
     * @param[in] index   Bone coefficient index (no check for validity of index).
     */
    public Quaternion bone_rotation(int index) { return m_bone_rotations[index]; }

    /**
     * @brief Set the quaternion of a bone.
     * @param[in] index           Bone index (no check for validity of index).
     * @param[in] value    New bone quaternion.
     */
    public void set_bone_rotation(int index, Quaternion q) { m_bone_rotations[index] = q; }
}
}

