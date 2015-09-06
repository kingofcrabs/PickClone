using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Configuration;
using System.Threading;

namespace Do3Acquier
{
    public class ImageAcquirer
    {
        public string m_sCameraName;
        public emDSRunMode m_Runmode;
        public int[] m_iCameraIDs;
        private string[] m_sCameraExpectedNameList;
        private string[] m_sCameraRealNameList;

        tDSCameraDevInfo[] pCameraInfo;
        bool bInitialized = false;
        Camera.DelegateProc psub = new Camera.DelegateProc(SnapThreadCallback);
        public  delegate  void Finished(string errMsg);
        public event Finished onFinished;
        static int frames = 0;
        readonly static int neededCameraCnt = 1;
        public static int SnapThreadCallback(int m_iCam, ref Byte pbyBuffer, ref tDSFrameInfo sFrInfo)
        {
            frames++;
            int pBmp24 = Camera.CameraISP(m_iCam, ref pbyBuffer, ref sFrInfo);
            Camera.CameraDisplayRGB24(m_iCam, pBmp24, ref sFrInfo);
            return 0;
        }

        private void Init()
        {
            if (bInitialized)
                return;
            m_iCameraIDs = new int[neededCameraCnt];
            //m_sCameraExpectedNameList = new string[] { "DSCD501000068", 
            //                                           "DSCD501000064" };    
            Stop();
            m_sCameraRealNameList = GetCameraList().ToArray();
            bool bok = CheckCameraNames(m_sCameraRealNameList.ToList());
            if (!bok)
            {
                throw new Exception("程序未注册！");
            }
            for( int i = 0; i< neededCameraCnt; i++)
            {
                string sName = m_sCameraRealNameList[i];
                emDSCameraStatus status = Camera.CameraInit(psub, sName, IntPtr.Zero, ref m_iCameraIDs[i]);
                if (status != emDSCameraStatus.STATUS_OK)
                    throw new Exception(string.Format("无法初始化相机，原因是: {0}", status.ToString()));
                Camera.CameraSetOnceWB(m_iCameraIDs[i]);
                //Camera.CameraSetMirror(m_iCameraIDs[i], emDSMirrorDirection.MIRROR_DIRECTION_HORIZONTAL, true);
                int resIndex = GetIndex(m_iCameraIDs[i]);
                //Camera.CameraSetImageSize(m_iCameraID, true, 0, 0, 1280, 960,1);
                Camera.CameraSetImageSizeSel(m_iCameraIDs[i], resIndex, true);
                Camera.CameraPlay(m_iCameraIDs[i]);
            }
            bInitialized = true;
        }

        private bool CheckCameraNames(List<string> sCameraNames)
        {
            return sCameraNames.Contains("USB2_CMOS_5M_F@CD501000021");
        }

        private int GetIndex(int id)
        {

            tDSCameraCapability dsCapbility = new tDSCameraCapability();
            tDSImageSize[] pImagesize = new tDSImageSize[8];
            Camera.CameraGetCapability(id, ref dsCapbility);
            List<string> capabilites = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                pImagesize[i].acDescription = new byte[64];
            }
            int pAddress = dsCapbility.pImageSizeDesc + 4;
            for (int j = 0; j < dsCapbility.iImageSizeDec; j++)
            {
                Camera.CopyMemory(Marshal.UnsafeAddrOfPinnedArrayElement(pImagesize[j].acDescription, 0), pAddress, 32);
                string sCapability = System.Text.Encoding.GetEncoding("GB2312").GetString(pImagesize[j].acDescription);
                if (sCapability.IndexOf("bin") != -1)
                    continue;
                capabilites.Add(sCapability);
                pAddress = pAddress + Marshal.SizeOf(pImagesize[j]);
            }

            for (int i = 0; i < capabilites.Count; i++)
            {
                if (capabilites[i].IndexOf("1280") != -1)
                    return i;

            }
            throw new Exception("相机不支持该分辨率!");
        }

        public void Stop()
        {
            if (m_iCameraIDs == null)
                return;

            try
            {
                for (int i = 0; i < neededCameraCnt; i++)
                {
                    Camera.CameraStop(m_iCameraIDs[i]);
                    Camera.CameraUnInit(m_iCameraIDs[i]);
                }
            }
            catch (Exception ex)
            {
                ;
            }
        }


        public void Start(string sFile, int cameraID)
        {
           
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                if (onFinished != null)
                    onFinished(ex.Message);
                return;
            }
            Thread.Sleep(1000);
            //if (!IsRightOrder(m_sCameraRealNameList[cameraID - 1], m_sCameraExpectedNameList[cameraID - 1]))
            //{
            //    cameraID = 3 - cameraID;
            //}
            if (File.Exists(sFile))
                File.Delete(sFile);
            string sOrgFile = sFile;
            int pos = sFile.IndexOf(".jpg");
            sFile = sFile.Substring(0, pos);
            emDSCameraStatus status = Camera.CameraCaptureFile(cameraID, sFile, (byte)emDSFileType.FILE_JPG, 100);
            while (true)
            {
                Thread.Sleep(50);
                if (File.Exists(sOrgFile))
                    break;
            }
            if (onFinished != null)
                onFinished("");
        }

        private bool IsRightOrder(string realName, string expectedName)
        {
            return realName.IndexOf(expectedName) != -1;
        }

        private List<string> GetCameraList()
        {
            pCameraInfo = new tDSCameraDevInfo[5]; //发送缓冲区大小可根据需要设置；
            for (int yy = 0; yy < 5; yy++)
            {
                pCameraInfo[yy] = new tDSCameraDevInfo();
                pCameraInfo[yy].acVendorName = new Byte[64];
                pCameraInfo[yy].acProductSeries = new Byte[64];
                pCameraInfo[yy].acProductName = new char[64];
                pCameraInfo[yy].acFriendlyName = new char[64];
                pCameraInfo[yy].acDevFileName = new Byte[64];
                pCameraInfo[yy].acFileName = new Byte[64];
                pCameraInfo[yy].acFirmwareVersion = new Byte[64];
                pCameraInfo[yy].acSensorType = new Byte[64];
                pCameraInfo[yy].acPortType = new Byte[64];
            }
            IntPtr[] ptArray = new IntPtr[1];
            ptArray[0] = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(tDSCameraDevInfo)) * 5);
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(tDSCameraDevInfo))*5);
            Marshal.Copy(ptArray, 0, pt, 1);
            
            int iNum = 0;
            Camera.CameraGetDevList(pt, ref iNum);
            if (iNum <= 0)
            {
                throw new Exception("没有找到相机！");
            }

            //if (iNum < 2)
            //{
            //    throw new Exception("只找到一个相机!");
            //}
            List<string> sCameraList = new List<string>();
            for (int i = 0; i < neededCameraCnt; i++)
            {
                string sCameraName = "";
                pCameraInfo[i] = (tDSCameraDevInfo)Marshal.PtrToStructure((IntPtr)((UInt32)pt + i * Marshal.SizeOf(typeof(tDSCameraDevInfo))), typeof(tDSCameraDevInfo));
                for (int j = 0; pCameraInfo[i].acFriendlyName[j] != '\0'; j++)
                {
                    sCameraName = sCameraName + pCameraInfo[i].acFriendlyName[j];
                }
                sCameraList.Add(sCameraName);
            }

            return sCameraList;
        }
    }
}
