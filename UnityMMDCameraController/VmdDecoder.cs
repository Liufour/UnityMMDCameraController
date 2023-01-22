using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class VmdDecoder
{
    private float Version = -1;
    private string MotionName = "None";

    public List<CameraKeyFrame> CameraFrames = new List<CameraKeyFrame>();

    public void LoadVmdFromFile(string filepath)
    {
        using (FileStream fileStream = File.OpenRead(filepath))
        using (BinaryReader binaryReader = new BinaryReader(fileStream))
        {
            LoadFromStream(binaryReader);
        }
    }
    public class CameraKeyFrame
    {
        public int Frame { get; private set; }
        public float Distance { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 RotationVec3 { get; private set; }
        public Vector2[] CurvePointX = new Vector2[2];
        public Vector2[] CurvePointY = new Vector2[2];
        public Vector2[] CurvePointZ = new Vector2[2];
        public Vector2[] CurvePointRotate = new Vector2[2];
        public Vector2[]CurvePointDistance = new Vector2[2];
        public Vector2[] CurvePointAngle = new Vector2[2];

        public int Angle;
        public bool Perspective;

        public CameraKeyFrame() { }
        public CameraKeyFrame(BinaryReader binaryReader) { Read(binaryReader); }

        public void Read(BinaryReader binaryReader)
        {
            Frame = binaryReader.ReadInt32();

            Distance = binaryReader.ReadSingle();

            Position = Util.ReadVector3(binaryReader);

            float rx = binaryReader.ReadSingle();
            float ry = binaryReader.ReadSingle();
            float rz = binaryReader.ReadSingle();
            RotationVec3 = new Vector3(rx, ry, rz);

            GetCurvePoint(binaryReader, CurvePointX);
            GetCurvePoint(binaryReader, CurvePointY);
            GetCurvePoint(binaryReader, CurvePointZ);
            GetCurvePoint(binaryReader, CurvePointRotate);
            GetCurvePoint(binaryReader, CurvePointDistance);
            GetCurvePoint(binaryReader, CurvePointAngle);
            Angle = binaryReader.ReadInt32();
            Perspective = BitConverter.ToBoolean(binaryReader.ReadBytes(1), 0);
        }
        void GetCurvePoint(BinaryReader binaryReader, Vector2[] points)
        {


            byte pointx0 = binaryReader.ReadByte();
            byte pointx1 = binaryReader.ReadByte();
            byte pointy0 = binaryReader.ReadByte();
            byte pointy1 = binaryReader.ReadByte();

            points[0].x = pointx0 / 127.0f;
            points[0].y = pointy0 / 127.0f;
            points[1].x = pointx1 / 127.0f;
            points[1].y = pointy1 / 127.0f;
        }
    };

    private void LoadFromStream(BinaryReader binaryReader)
    {
        char[] buffer = new char[30];

        string RightFileType = "Vocaloid Motion Data";
        byte[] fileTypeBytes = binaryReader.ReadBytes(30);
        string fileType = System.Text.Encoding.GetEncoding("shift_jis").GetString(fileTypeBytes).Substring(0, RightFileType.Length);
        if (!fileType.Equals("Vocaloid Motion Data"))
        {
            Debug.Log("读取的文件并不是VMD动作文件");
        }
        Version = BitConverter.ToSingle((from c in buffer select Convert.ToByte(c)).ToArray(), 0);
        byte[] nameBytes = binaryReader.ReadBytes(20);
        MotionName = System.Text.Encoding.GetEncoding("shift_jis").GetString(nameBytes);
        MotionName = MotionName.TrimEnd('\0').TrimEnd('?').TrimEnd('\0');

        int boneFrameCount = binaryReader.ReadInt32();


        int faceFrameCount = binaryReader.ReadInt32();

        int cameraFrameCount = binaryReader.ReadInt32();
        for (int i = 0; i < cameraFrameCount; i++)
        {
            CameraFrames.Add(new CameraKeyFrame(binaryReader));
        }
        CameraFrames = CameraFrames.OrderBy(x => x.Frame).ToList();

        int lightFrameCount = binaryReader.ReadInt32();

        if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length) { return; }

        int selfShadowFrameCount = binaryReader.ReadInt32();

        if (binaryReader.BaseStream.Position == binaryReader.BaseStream.Length) { return; }

        int ikFrameCount = binaryReader.ReadInt32();

    }

    class Util
    {
        public static Vector3 ReadVector3(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();
            return new Vector3(x, y, z);
        }

        public static Quaternion ReadQuaternion(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();
            float w = binaryReader.ReadSingle();

            return new Quaternion(x, y, z, w);
        }

        public static Quaternion ReadEulerQuaternion(BinaryReader binaryReader)
        {
            float x = -binaryReader.ReadSingle();
            float y = binaryReader.ReadSingle();
            float z = -binaryReader.ReadSingle();

            return Quaternion.Euler(x, y, z);
        }
    }
    
}

      
