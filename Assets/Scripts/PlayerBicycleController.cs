using UnityEngine;
using UnityEngine.UI;
using System.IO.Ports;

public class PlayerBicycleController : BicycleController {
    public bool useSensors;
    public bool userHasControl;
    private SerialPort _stream;

    void Awake() {
        if (useSensors) {
            OpenStream();
        }

        userHasControl = false;
        powerInput = 0;
        steeringInput = 0;
        frontBrake = false;
        rearBrake = false;
        previousDesiredZRot = 0;

        tireAudioRoad = SetUpAudioSource(tireAudioRoadClip);
        tireAudioDirt = SetUpAudioSource(tireAudioDirtClip);
        pedalingAudio = SetUpAudioSource(pedalingAudioClip);
        idleAudio = SetUpAudioSource(idleAudioClip);
    }

    public void SetSensors(bool value) {
        useSensors = value;
        if (useSensors)
            OpenStream();
        else {
            if (_stream != null)
                _stream.Close();
            _stream = null;
        }
    }
    
    protected void OpenStream() {
        _stream = new SerialPort("COM4", 9600);
        _stream.ReadTimeout = 10;
        _stream.Open();
    }

    protected override void getInputs() {
        if (userHasControl) {
            if (useSensors) {
                float targetRpm = ReadRpmFromArduino();
                float rpmDiff = targetRpm - rearWheelCollider.rpm;
                powerInput = Mathf.Clamp(rpmDiff / 50f, -1, 1);
                steeringInput = ReadSteeringFromArduino();
                turningAngle = steeringInput * maxTurnAngle;
            }
            else {
                powerInput = Input.GetAxis("Vertical");
                steeringInput = Input.GetAxis("Horizontal");
                // When using keys, we only have "digital" input, it's either on of off, so we need to attenuate the turn
                // amount according to the speed the bike is travelling, to avoid sharp changes in direction that would
                // render the bike unusable.
                float adjustedSpeed = Mathf.Clamp(rigidBody.velocity.magnitude - 1f, 0.01f, Mathf.Infinity);
                float turnSqrtCoef = Mathf.Sqrt(2.8f * adjustedSpeed);
                float turnQuota = Mathf.Clamp(1f / turnSqrtCoef - 0.1f, 0f, 1f);
                turningAngle = steeringInput * maxTurnAngle * turnQuota;
            }

            // When using sensors, brakes are not needed, because when the user brakes his bike, the wheel will stop,
            // prompting the script to also force the wheels to stop, thus braking.
            frontBrake = Input.GetButton("FrontBrake");
            rearBrake = Input.GetButton("RearBrake");
        }
        else {
            frontBrake = true;
            rearBrake = true;
        }
    }

    protected float ReadSteeringFromArduino() {
        _stream.Write("a"); // Write 'a' to signal Arduino that we want turn values
        return ReadByteFromArduino() / -128.0f;
    }

    protected float ReadRpmFromArduino() {
        _stream.Write("b"); // Write 'b' to signal Arduino that we want RPM values
        return Mathf.Clamp(ReadByteFromArduino(), 0, 500);
    }

    protected float ReadByteFromArduino() {
        byte[] floatArray = new byte[4];
        floatArray[0] = (byte) _stream.ReadByte();
        floatArray[1] = (byte) _stream.ReadByte();
        floatArray[2] = (byte) _stream.ReadByte();
        floatArray[3] = (byte) _stream.ReadByte();
        return System.BitConverter.ToSingle(floatArray, 0);
    }
}