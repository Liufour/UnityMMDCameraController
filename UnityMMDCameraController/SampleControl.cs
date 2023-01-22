using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SampleControl : MonoBehaviour
{
    private Animator charaAnimator;
    MMD4MCameraController cameraController;
    private void Start()
    {
        charaAnimator = gameObject.GetComponent<Animator>();
        cameraController = gameObject.GetComponent<MMD4MCameraController>();
    }
    public void PlayMotion()
    {
        if(cameraController!=null && charaAnimator!=null)
        {
            charaAnimator.SetTrigger("PlayMotion");
            cameraController.SetPlay();
        }

    }
}
[CanEditMultipleObjects, CustomEditor(typeof(SampleControl))]
public class CustomEditorTestEditorSampleControl : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Play"))
        {
            SampleControl sc = (SampleControl)target;
            sc.PlayMotion();
        }
    }
}