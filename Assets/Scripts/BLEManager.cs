using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Android;

public class BLEManager : MonoBehaviour
{
    private AndroidJavaObject bluetoothAdapter = null;
    private AndroidJavaObject bluetoothLeScanner = null;
    private bool isScanning = false;

    private List<string> esp32Devices = new List<string>();

    void Start()
    {
        // 请求必要的权限
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        // 初始化 Bluetooth
        InitializeBluetooth();
        StartScan();
    }

    private void InitializeBluetooth()
    {
        AndroidJavaClass bluetoothAdapterClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter");
        bluetoothAdapter = bluetoothAdapterClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");

        if (bluetoothAdapter == null)
        {
            Debug.LogError("Bluetooth is not supported on this device.");
            return;
        }

        bool isEnabled = bluetoothAdapter.Call<bool>("isEnabled");
        if (!isEnabled)
        {
            Debug.LogError("Bluetooth is not enabled.");
            return;
        }

        bluetoothLeScanner = bluetoothAdapter.Call<AndroidJavaObject>("getBluetoothLeScanner");

        if (bluetoothLeScanner == null)
        {
            Debug.LogError("Failed to get BluetoothLeScanner.");
            return;
        }

        Debug.Log("Bluetooth initialized successfully.");
    }

    private void StartScan()
    {
        if (!isScanning && bluetoothLeScanner != null)
        {
            isScanning = true;
            bluetoothLeScanner.Call("startScan", new ScanCallback(OnDeviceFound));
            Debug.Log("Started scanning for BLE devices.");
        }
    }

    private void StopScan()
    {
        if (isScanning && bluetoothLeScanner != null)
        {
            bluetoothLeScanner.Call("stopScan", new ScanCallback(OnDeviceFound));
            isScanning = false;
            Debug.Log("Stopped scanning for BLE devices.");
        }
    }

    private void OnDeviceFound(string deviceName, string deviceAddress)
    {
        if (deviceName.Contains("ESP32") && !esp32Devices.Contains(deviceAddress))
        {
            Debug.Log("ESP32 device found: " + deviceAddress);
            esp32Devices.Add(deviceAddress);
        }

        // 假设您有3个ESP32设备，您可以根据需要调整这个数字
        if (esp32Devices.Count !=0)
        {
            StopScan();
            foreach (string address in esp32Devices)
            {
                ConnectToDevice(address);
            }
        }
    }

    private void ConnectToDevice(string deviceAddress)
    {
        AndroidJavaObject device = bluetoothAdapter.Call<AndroidJavaObject>("getRemoteDevice", deviceAddress);
        AndroidJavaObject gatt = device.Call<AndroidJavaObject>("connectGatt", null, false, new GattCallback(deviceAddress));

        Debug.Log("Connecting to device: " + deviceAddress);
    }

    private class GattCallback : AndroidJavaProxy
    {
        private string deviceAddress;

        public GattCallback(string address) : base("android.bluetooth.BluetoothGattCallback")
        {
            this.deviceAddress = address;
        }

        void onConnectionStateChange(AndroidJavaObject gatt, int status, int newState)
        {
            if (newState == 2) // BluetoothProfile.STATE_CONNECTED
            {
                Debug.Log("Connected to GATT server: " + deviceAddress);
                gatt.Call("discoverServices");
            }
            else if (newState == 0) // BluetoothProfile.STATE_DISCONNECTED
            {
                Debug.Log("Disconnected from GATT server: " + deviceAddress);
            }
        }

        void onServicesDiscovered(AndroidJavaObject gatt, int status)
        {
            if (status == 0) // BluetoothGatt.GATT_SUCCESS
            {
                Debug.Log("Services discovered on device: " + deviceAddress);

                // 使用字符串形式的 UUID
                string serviceUUID = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
                string characteristicUUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8";

                AndroidJavaObject service = gatt.Call<AndroidJavaObject>("getService", new AndroidJavaObject("java.util.UUID", new System.Guid(serviceUUID).ToByteArray()));
                if (service != null)
                {
                    AndroidJavaObject characteristic = service.Call<AndroidJavaObject>("getCharacteristic", new AndroidJavaObject("java.util.UUID", new System.Guid(characteristicUUID).ToByteArray()));
                    if (characteristic != null)
                    {
                        // 发送消息到设备
                        string message = "Hello ESP32";
                        byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes(message);
                        characteristic.Call("setValue", dataToSend);
                        gatt.Call("writeCharacteristic", characteristic);

                        Debug.Log("Message sent to device: " + deviceAddress);

                        // 发送完成后可以选择断开连接
                        gatt.Call("disconnect");
                    }
                }
            }
        }

    }

    private class ScanCallback : AndroidJavaProxy
    {
        private System.Action<string, string> onDeviceFoundCallback;

        public ScanCallback(System.Action<string, string> onDeviceFound)
            : base("android.bluetooth.le.ScanCallback")
        {
            this.onDeviceFoundCallback = onDeviceFound;
        }

        // 当扫描到设备时，这个方法会被调用
        void onScanResult(int callbackType, AndroidJavaObject result)
        {
            AndroidJavaObject device = result.Call<AndroidJavaObject>("getDevice");
            string deviceName = device.Call<string>("getName");
            string deviceAddress = device.Call<string>("getAddress");

            if (deviceName != null && onDeviceFoundCallback != null)
            {
                onDeviceFoundCallback.Invoke(deviceName, deviceAddress);
            }
        }
    }
}
