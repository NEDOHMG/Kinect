using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Ports;

public class PGMactuation : MonoBehaviour
{

    //change the COM port number according to the system
    public SerialPort serial = new SerialPort("COM5", 9600);

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Console.WriteLine("key 1 pressed!");
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("1");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("1");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("2");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("2");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("3");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("3");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("4");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("4");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("5");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("5");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("6");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("6");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("7");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("7");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("8");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("8");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("9");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("9");
            }
        }

		else if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("0");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("0");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F1))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("a");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("a");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F2))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("b");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("b");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F3))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("c");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("c");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F4))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("d");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("d");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F5))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("e");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("e");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F6))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("f");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("f");
            }
        }

		else if (Input.GetKeyDown(KeyCode.F7))
        {
            if (serial.IsOpen == false)
            {
                serial.Open();
                serial.Write("g");
            }

            else if (serial.IsOpen == true)
            {
                serial.Write("g");
            }
        }

    }
}
