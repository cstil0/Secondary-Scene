using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;
using System.IO;
using System.Globalization;

public class UDPReceiver : MonoBehaviour
{
    // udpclient object
    UdpClient client;
    public int serverPort;
    Thread receiveThread;
    String receivedMessage;

    public Camera ScreenCamera;
    Vector3 startPos;
    Vector3 remoteStartPos;
    Vector3 currPos;

    Vector3 startRot;
    Vector3 remoteStartRot;
    Vector3 currRot;

    // main thread that listens to UDP messages through a defined port
    void UDPTest()
    {
        // create client and set the port (HARCODEADO EN EL EDITOR!!-----)
        client = new UdpClient("192.168.0.28", serverPort);
        // loop needed to keep listening
        while (true)
        {
            try
            {
                // recieve messages through the end point
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.17"), serverPort);
                byte[] receiveBytes = client.Receive(ref remoteEndPoint);
                // once the message is recieved, encode it as ASCII
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log("Position: " + receivedMessage);

                string[] splittedMessage = receivedMessage.Split(" ");
                currPos = new Vector3(float.Parse(splittedMessage[0], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture));

                if (remoteStartPos == null)
                    if (remoteStartPos == new Vector3(0.0f, 0.0f, 0.0f))
                    {
                        remoteStartPos = new Vector3(currPos.x, currPos.y, currPos.z);
                    }

                receiveBytes = client.Receive(ref remoteEndPoint);
                // once the message is recieved, encode it as ASCII
                receivedMessage = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log("Rotation: " + receivedMessage);

                splittedMessage = receivedMessage.Split(" ");
                currRot = new Vector3(float.Parse(splittedMessage[0], CultureInfo.InvariantCulture), float.Parse(splittedMessage[1], CultureInfo.InvariantCulture), float.Parse(splittedMessage[2], CultureInfo.InvariantCulture));

                if (remoteStartRot == new Vector3(0.0f, 0.0f, 0.0f))
                {
                    remoteStartRot = new Vector3(currRot.x, currRot.y, currRot.z);
                }
            }
            catch (Exception e)
            {
                print("Exception thrown " + e.Message);
            }
        }
    }
    void OnDisable()
    {
        client.Close();
        // stop thread when object is disabled
        if (receiveThread != null)
            receiveThread.Abort();
    }
    // Start is called before the first frame update
    void Start()
    {
        // Start thread to listen UDP messages and set it as background
        receiveThread = new Thread(UDPTest);
        receiveThread.IsBackground = true;
        receiveThread.Start();

        startPos = ScreenCamera.transform.position;
        startRot = ScreenCamera.transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (receivedMessage != null)
        {
            Vector3 remotePosDiff = currPos - remoteStartPos;
            ScreenCamera.transform.position = remotePosDiff + startPos;
            ScreenCamera.transform.Rotate(currRot);

            Vector3 remoteRotDiff = remoteStartRot - currRot;
            ScreenCamera.transform.rotation = Quaternion.Euler(remoteRotDiff + startRot);
        }
    }
}