using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Playables;

public class MMD4MCameraController : MonoBehaviour
{
    //本脚本的功能是读取MMD的相机动作VMD文件，并在Unity中根据MMD模型动作动画播放帧数获取当前的相机变换从而控制相机位置与旋转

    //使用方法：
    //拖拽该脚本到场景中MMD4Mecanim转换的模型物体上
    //将主相机拖到该脚本组件的MainCamera上
    //右键VMD文件点击Copy Path，将路径粘贴到物体该组件的VmdFilePath中
    //当角色动画开始播放时，设置isPlaying为true，即可根据角色动画同步控制相机
    
    //注：
    //该脚本需要角色动作最后一个关键帧的帧数大于相机动作最后一个关键帧的帧数，可通过在MMD软件中给角色动作最后加空帧解决
    //本脚本没有实现在Unity中实时任意跳转关键帧预览相机变换的功能，只能从头播放到尾
    //可能会出现相机与MMD模型旋转了180度的情况，设置MMD模型Y轴旋转180即可解决
    //Scale为MMD与Unity之间的坐标缩放比例，用于相机位置向量缩放，默认值12.5为本人的粗略测算，可以自由调节

    private int currCameraKeyFrameIndex = 0;
    private bool isPlaying;
    private Animator charaAnimator;
    private VmdDecoder VmdData;

    public Camera MainCamera;
    public string VmdFileName;
    public float Scale = 12.5f;

    void Start()
    {
        charaAnimator = gameObject.GetComponent<Animator>();
        VmdData = new VmdDecoder();
        VmdData.LoadVmdFromFile(VmdFileName);
        SetCamera(0.0f);
    }

    public void SetPlay()
    {
        StartCoroutine(StartPlay());
    }
    IEnumerator StartPlay()
    {
        yield return null;
        if (isPlaying == false)
        {
            isPlaying = true;
        }
    }
    void Update()
    {
        if (isPlaying == true)
        {
            float currentTime = charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float length = charaAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
            float frameRate = charaAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
            float totalFrame = length / (1 / frameRate);
    
            SetCamera(totalFrame * currentTime);
        }
        
    }

    public void SetCamera(float frame)
    {
        if (frame > VmdData.CameraFrames[VmdData.CameraFrames.Count-1].Frame)
        {
            Vector3 center = VmdData.CameraFrames[VmdData.CameraFrames.Count - 1].Position;
            Vector3 rotation = VmdData.CameraFrames[VmdData.CameraFrames.Count - 1].RotationVec3;
            float angle = VmdData.CameraFrames[VmdData.CameraFrames.Count - 1].Angle;
            float distance = VmdData.CameraFrames[VmdData.CameraFrames.Count - 1].Distance;
            MainCamera.transform.position = center / Scale;
            rotation *= 180.0f / 3.1415926f;
            MainCamera.transform.rotation = Quaternion.Euler(new Vector3(-rotation.x, -rotation.y + 180.0f, -rotation.z));
            MainCamera.transform.Translate(Vector3.back * -distance / Scale);
            MainCamera.fieldOfView = angle;
            return;
        }
        CheckKeyFrame(frame);
        if (currCameraKeyFrameIndex >= VmdData.CameraFrames.Count - 1)
        {
            currCameraKeyFrameIndex = VmdData.CameraFrames.Count - 1;
        }
        if (VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Frame - VmdData.CameraFrames[currCameraKeyFrameIndex].Frame == 1)
        {
            frame = VmdData.CameraFrames[currCameraKeyFrameIndex].Frame;
        }
        float progress = frame - VmdData.CameraFrames[currCameraKeyFrameIndex].Frame;

        progress /= VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Frame - VmdData.CameraFrames[currCameraKeyFrameIndex].Frame;
        float currprogressX = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointX[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointX[1],progress);
        float currposX = Mathf.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].Position.x,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Position.x,
           currprogressX);
        float currprogressY = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointY[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointY[1], progress);
        float currposY = Mathf.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].Position.y,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Position.y,
            currprogressY);
        float currprogressZ = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointZ[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointZ[1], progress);
        float currposZ = Mathf.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].Position.z,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Position.z,
            currprogressZ);
        float currprogressR = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointRotate[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointRotate[1], progress);
        Vector3 currRotation = Vector3.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].RotationVec3,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].RotationVec3,
           currprogressR);
        float currprogressD = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointDistance[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointDistance[1], progress);
        float currDistance = Mathf.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].Distance,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Distance,
           currprogressD);
        float currprogressA = Bezier(VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointAngle[0],
            VmdData.CameraFrames[currCameraKeyFrameIndex + 1].CurvePointAngle[1], progress);
        float currAngle = Mathf.Lerp(
           VmdData.CameraFrames[currCameraKeyFrameIndex].Angle,
           VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Angle,
           currprogressA);
        Vector3 camcenter = new Vector3(currposX, currposY, currposZ);

        MainCamera.transform.position = camcenter / Scale;
        currRotation *= 180.0f / 3.1415926f;
        MainCamera.transform.rotation = Quaternion.Euler(new Vector3(-currRotation.x, -currRotation.y + 180.0f, -currRotation.z));
        MainCamera.transform.Translate(Vector3.back * -currDistance / Scale);
        MainCamera.fieldOfView = currAngle;

    }
    private void CheckKeyFrame(double frame)
    {
        if(frame> VmdData.CameraFrames[VmdData.CameraFrames.Count - 1].Frame)
        {
            return;
        }
        if (frame >= VmdData.CameraFrames[currCameraKeyFrameIndex + 1].Frame)
        {
            currCameraKeyFrameIndex += 1;
            CheckKeyFrame(frame);

        }
        if (frame < VmdData.CameraFrames[currCameraKeyFrameIndex].Frame)
        {
            currCameraKeyFrameIndex -= 1;
            CheckKeyFrame(frame);

        }
    }
    float Bezier(Vector2 p1, Vector2 p2, float t)
    {

        Vector2 p0 = new Vector2(0.0f, 0.0f);
        Vector2 p3 = new Vector2(1.0f, 1.0f);

        Vector2 result;
        result = p0 * Mathf.Pow(1.0f - t, 3)
            + 3.0f * p1 * t * Mathf.Pow(1.0f - t, 2)
            + 3.0f * p2 * t * t * (1.0f - t)
            + p3 * t * t * t;

        return result.y;
    }

}
