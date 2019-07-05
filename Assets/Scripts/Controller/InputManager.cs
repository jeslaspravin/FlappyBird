using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input manager that holds the input mapping and will be responsible for inputs data being passed on to listeners.
/// </summary>
public class InputManager : MonoBehaviour
{

    /// <summary>
    /// Input set data struct ,Set of inputs that needs to be controllable in same way can be in single set.
    /// </summary>
    struct InputSetStruct
    {
        /// <summary>
        /// Name of input list this set listens to.
        /// </summary>
        public List<string> inputName;

        /// <summary>
        /// For each of inputName item there will be a callback that gets invoked with value of input.
        /// </summary>
        public List<Action<float>> callbacks;

        /// <summary>
        /// Can be used to control whether to listen to this set.
        /// </summary>
        public bool bIsActiveSet;
        public InputSetStruct(List<string> inputNameList, List<Action<float>> callbackList)
        {
            bIsActiveSet = true;
            inputName = inputNameList;
            callbacks = callbackList;
        }
        public InputSetStruct(string firstInputName, Action<float> firstCallback)
        {
            bIsActiveSet = true;
            inputName = new List<string>();
            callbacks = new List<Action<float>>();
            inputName.Add(firstInputName);
            callbacks.Add(firstCallback);
        }

        public InputSetStruct(string firstInputName, int index, Action<float> firstCallback)
        {
            bIsActiveSet = true;
            inputName = new List<string>(10);
            callbacks = new List<Action<float>>(10);
            inputName.Insert(index, firstInputName);
            callbacks.Insert(index, firstCallback);
        }

        public void setIsActiveSet(bool active)
        {
            bIsActiveSet = active;
        }
    }
    /// <summary>
    /// Dictionary of all input sets that user controller listens to. for axis only
    /// </summary>
    private Dictionary<string, InputSetStruct> inputAxisSet = new Dictionary<string, InputSetStruct>();

    private Dictionary<string, InputSetStruct> touchAxisSet = new Dictionary<string, InputSetStruct>();

    /// <summary>
    /// Can be used to pause the controller all together using this global switch.
    /// </summary>
    private bool bGlobalPause;

    /// <summary>
    /// Delegate for processed input event
    /// </summary>
    public delegate void InputManagerDelegate();

    /// <summary>
    /// Event that gets triggered once all input events are processed.
    /// </summary>
    public event InputManagerDelegate onInputProcessed;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        foreach (string key in inputAxisSet.Keys)
        {
            for (int i = 0; i < inputAxisSet[key].callbacks.Count; i++)
            {
                inputAxisSet[key].callbacks[i]((!bGlobalPause && inputAxisSet[key].bIsActiveSet) ? Input.GetAxis(inputAxisSet[key].inputName[i]) : 0);
            }
        }
        if(Input.touchSupported)
        {
            if (Input.touchPressureSupported)
            {
                foreach (string key in touchAxisSet.Keys)
                {
                    for (int i = 0; i < touchAxisSet[key].callbacks.Count && touchAxisSet[key].callbacks[i] != null; i++)
                    {
                        if (Input.touchCount <= i)
                        {
                            touchAxisSet[key].callbacks[i](0);
                            continue;
                        }
                        Touch touch = Input.GetTouch(i);
                        float touchValue = touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended ? touch.pressure / touch.maximumPossiblePressure : 0;

                        touchAxisSet[key].callbacks[i]((!bGlobalPause && inputAxisSet[key].bIsActiveSet) ? touchValue : 0);
                    }
                }
            }
            else
            {
                foreach (string key in touchAxisSet.Keys)
                {
                    for (int i = 0; i < touchAxisSet[key].callbacks.Count; i++)
                    {
                        if (Input.touchCount <= i)
                        {
                            touchAxisSet[key].callbacks[i](0);
                            continue;
                        }
                        Touch touch = Input.GetTouch(i);
                        // Scaling touch pressure based on duration of press in case of unsupported devices.
                        float touchValue = touch.phase != TouchPhase.Canceled && touch.phase != TouchPhase.Ended ? 1 : 0;
                        touchAxisSet[key].callbacks[i]((!bGlobalPause && inputAxisSet[key].bIsActiveSet) ? touchValue : 0);
                    }
                }
            }
        }
        if (onInputProcessed != null)
            onInputProcessed.Invoke();
    }

    /// <summary>
    /// Adds listener to set of particular name in the Input manager.
    /// </summary>
    /// <param name="setName">Name of Set</param>
    /// <param name="inputName">Input name to map into the set</param>
    /// <param name="methodCallback">Input callback to map into the set</param>
    public void addToListenerSet(string setName, string inputName, Action<float> methodCallback)
    {
        addToAxisSet(setName, inputName, methodCallback, ref inputAxisSet);
    }
    public void addToTouchSet(string setName, string inputName, Action<float> methodCallback)
    {
        int idx = System.Int32.Parse(inputName);

        if (touchAxisSet.ContainsKey(setName))
        {
            touchAxisSet[setName].inputName.Insert(idx, inputName);
            touchAxisSet[setName].callbacks.Insert(idx, methodCallback);
        }
        else
        {
            InputSetStruct set = new InputSetStruct(inputName, idx, methodCallback);
            touchAxisSet.Add(setName, set);
        }

    }

    private void addToAxisSet(string setName, string inputName, Action<float> methodCallback, ref Dictionary<string, InputSetStruct> axisSet)
    {

        if (axisSet.ContainsKey(setName))
        {
            axisSet[setName].inputName.Add(inputName);
            axisSet[setName].callbacks.Add(methodCallback);
        }
        else
        {
            InputSetStruct set = new InputSetStruct(inputName, methodCallback);
            axisSet.Add(setName, set);
        }
    }

    /// <summary>
    /// Removes listening to the set of given name entirely.
    /// </summary>
    /// <param name="setName">Set name to stop listening to</param>
    /// <returns>True on successfully stopping</returns>
    public bool stopListenerSet(string setName)
    {
        return inputAxisSet.Remove(setName);
    }

    public bool stopTouchSet(string setName)
    {
        return touchAxisSet.Remove(setName);
    }

    /// <summary>
    /// Stops listening to certain input of given name alone from set of given name.
    /// </summary>
    /// <param name="setName">Set name to look for input</param>
    /// <param name="inputName">Input name to remove</param>
    /// <returns>True on success</returns>
    public bool stopListenerSet(string setName, string inputName)
    {
        return stopAxisSet(setName, inputName, ref inputAxisSet);
    }

    public bool stopTouchSet(string setName, string inputName)
    {
        return stopAxisSet(setName, inputName, ref inputAxisSet);
    }

    private bool stopAxisSet(string setName, string inputName, ref Dictionary<string, InputSetStruct> axisSet)
    {
        if (axisSet.ContainsKey(setName))
        {
            int index = axisSet[setName].inputName.FindIndex((string val) => { return val.Equals(inputName); });
            if (index != -1)
            {
                axisSet[setName].inputName.RemoveAt(index);
                axisSet[setName].callbacks.RemoveAt(index);
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Pauses listening,The entire input manager's global switch for input listening is turned off
    /// </summary>
    public void pauseInputs()
    {
        bGlobalPause = true;
    }

    /// <summary>
    /// Resumes the input manager.Flicks global switch back on.
    /// </summary>
    public void resumeInputs()
    {
        bGlobalPause = false;
    }

    public bool isInputsActive()
    {
        return !bGlobalPause;
    }

    /// <summary>
    /// Pauses input set of given name.
    /// </summary>
    /// <param name="setName">Name of input set to pause</param>
    /// <returns>True on success</returns>
    public bool pauseListenerSet(string setName)
    {
        return pauseAxisSet(setName, ref inputAxisSet);
    }
    public bool pauseTouchSet(string setName)
    {
        return pauseAxisSet(setName, ref touchAxisSet);
    }


    private bool pauseAxisSet(string setName, ref Dictionary<string, InputSetStruct> axisSet)
    {
        InputSetStruct availableSet = new InputSetStruct();
        if (axisSet.TryGetValue(setName, out availableSet))
        {
            availableSet.bIsActiveSet = false;
            axisSet[setName] = availableSet;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Resumes input set of given name.
    /// </summary>
    /// <param name="setName">Name of input set to resume</param>
    /// <returns>True on success</returns>
    public bool resumeListenerSet(string setName)
    {
        return resumeAxisSet(setName, ref inputAxisSet);
    }
    public bool resumeTouchSet(string setName)
    {
        return resumeAxisSet(setName, ref touchAxisSet);
    }


    private bool resumeAxisSet(string setName, ref Dictionary<string, InputSetStruct> axisSet)
    {
        InputSetStruct availableSet = new InputSetStruct();
        if (axisSet.TryGetValue(setName, out availableSet))
        {
            availableSet.bIsActiveSet = true;
            axisSet[setName] = availableSet;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Check whether input set is actively listening
    /// </summary>
    /// <param name="setName">Name of set to check</param>
    /// <returns>True when set is active</returns>
    public bool isInputSetActive(string setName)
    {
        InputSetStruct availableSet = new InputSetStruct();
        if (inputAxisSet.TryGetValue(setName, out availableSet))
        {
            return !availableSet.bIsActiveSet;
        }
        return false;
    }


}
