/*
 *  Copyright 2013 faceshift AG. All rights reserved.
 */
using UnityEngine;
using System;
using System.Collections.Generic;

namespace fs
{
/**
 * @brief class Clip contains a Rig and a list of RigStates defining an animation. The states are instances of a Rig that defines the blendshape and bone names.
 */
public class Clip
{
    //! Rig determining the blendshape and bone names.
    private Rig m_rig;

    //! List of states determining the animation curves of blendshapes and bones.
    private List<RigState> m_states;

    /**
     * @brief Create a new clip with a specific rig.
     * @param[in] rig     The rig of the clip.
     */
    public Clip (Rig rig)
    {
        m_rig = rig;
        m_states = new List<RigState>();
    }

    /**
     * @brief Creates a new state and adds it to this clip.
     * @param[in] timestamp    The timestamp for this new state.
     * @return The newly created state.
     */
    public RigState new_state(double timestamp)
    {
        RigState state = new RigState(m_rig);
        state.set_timestamp(timestamp);
        m_states.Add(state);
        return state;
    }

    //! @brief Returns the rig of this clip
    public Rig rig()
    {
        return m_rig;
    }

    /**
     * @brief Return number of states of the clip.
     * @return The number of states of the clip.
     */
    public int num_states() { return m_states.Count; }

    //! @brief Get state. No check of index.
    public RigState this[int index]
    {
        get { return m_states[index]; }
    }
}
}

