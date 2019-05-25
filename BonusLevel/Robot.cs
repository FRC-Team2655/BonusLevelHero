using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix;
using Math = System.Math;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;

namespace BonusLevel {
    public class Robot {
        bool haveEnabledButtonsBeenReleased = true;

        public enum State { Enabled, Disabled, None };

        GameController js0 = null;

        CTRE.Phoenix.PneumaticControlModule pcm = new PneumaticControlModule(0);

        TalonSRX leftMaster = new TalonSRX(RobotMap.LEFT_MASTER);
        //TalonSRX leftSlave1 = new TalonSRX(RobotMap.LEFT_SLAVE1);
        //TalonSRX leftSlave2 = new TalonSRX(RobotMap.LEFT_SLAVE2);
        //TalonSRX rightMaster = new TalonSRX(RobotMap.RIGHT_MASTER);
        //TalonSRX rightSlave1 = new TalonSRX(RobotMap.RIGHT_SLAVE1);
        //TalonSRX rightSlave2 = new TalonSRX(RobotMap.RIGHT_SLAVE2);

        private State currentState = State.None, newState = State.Disabled;

        private void robotInit() {
            js0 = new GameController(UsbHostDevice.GetInstance());
            //leftSlave1.Follow(leftMaster);
            //leftSlave2.Follow(leftMaster);
            //rightSlave1.Follow(rightMaster);
            //rightSlave2.Follow(rightMaster);
        }

        private void robotPeriodic() {
            // If both buttons have been pressed, enable the robot
            if (js0.GetButton(RobotMap.BACK) && js0.GetButton(RobotMap.START) && currentState == State.Disabled) {
                switchState(State.Enabled);
                haveEnabledButtonsBeenReleased = false;
            }

            // If both buttons have been released, allow the ability to disable
            if (!js0.GetButton(RobotMap.BACK) && !js0.GetButton(RobotMap.START) && !haveEnabledButtonsBeenReleased) {
                haveEnabledButtonsBeenReleased = true;
            }

            // If either button has been pressed, disable
            if (haveEnabledButtonsBeenReleased && (js0.GetButton(RobotMap.BACK) || js0.GetButton(RobotMap.START)) && currentState == State.Enabled) {
                switchState(State.Disabled);
            }

            pcm.StartCompressor();
        }

        private void enabledInit() {
            Debug.Print("Enabled");
            leftMaster.SetNeutralMode(NeutralMode.Coast);
            //rightMaster.SetNeutralMode(NeutralMode.Coast);
        }

        private void enabledPeriodic() {
            // 2 - left and right, right stick
            // 5 - up and down, right stick
            float power = -1 * js0.GetAxis(5);
            double rotate = 0.4 * js0.GetAxis(2);
            drivePercentage(power, (float)rotate);
        }

        private void disabledInit() {
            Debug.Print("Disabled");
            leftMaster.SetNeutralMode(NeutralMode.Brake);
            //rightMaster.SetNeutralMode(NeutralMode.Brake);
        }

        private void disabledPeriodic() {
            drivePercentage(0, 0);
        }

        double[] arcadeDrive(float xSpeed, float zRotation) {
            float leftMotorOutput;
            float rightMotorOutput;

            // Prevent -0 from breaking the arcade drive...
            xSpeed += 0.0f;
            zRotation += 0.0f;

            float maxInput = (float) Math.Max(Math.Abs(xSpeed), Math.Abs(zRotation));
            // If xSpeed is negative make maxInput negative
            if (xSpeed < 0) maxInput *= -1;

            if (xSpeed >= 0.0) {
                // First quadrant, else second quadrant
                if (zRotation >= 0.0) {
                    leftMotorOutput = maxInput;
                    rightMotorOutput = xSpeed - zRotation;
                } else {
                    leftMotorOutput = xSpeed + zRotation;
                    rightMotorOutput = maxInput;
                }
            } else {
                // Third quadrant, else fourth quadrant
                if (zRotation >= 0.0) {
                    leftMotorOutput = xSpeed + zRotation;
                    rightMotorOutput = maxInput;
                } else {
                    leftMotorOutput = maxInput;
                    rightMotorOutput = xSpeed - zRotation;
                }
            }

            return new double[] { leftMotorOutput, rightMotorOutput };
        }

        public void feed() {
            // Run robotInit if there is no current state
            if (currentState == State.None) {
                robotInit();
            }

            // Change the current state (and run init funciton) if there is a new state
            if (newState != currentState) {
                currentState = newState;

                switch(currentState) {
                    case State.Enabled:
                        enabledInit();
                        break;
                    case State.Disabled:
                        disabledInit();
                        break;
                }
            }

            switch (currentState) {
                case State.Enabled:
                    enabledPeriodic();
                    break;
                case State.Disabled:
                    disabledPeriodic();
                    break;
            }
            periodicSafetyCheck();
            robotPeriodic();
        }

        public void switchState(State state) {
            // Do not allow enable without the game controller being enabled
            if (state == State.Enabled && js0.GetConnectionStatus() == UsbDeviceConnection.NotConnected) {
                return;
            } 
            newState = state;
        }

        UsbDeviceConnection lastConnectionStatus = UsbDeviceConnection.NotConnected;
        public void periodicSafetyCheck() {
            if (currentState == State.Enabled) {
                // Keeps talonsrx enabled
                CTRE.Phoenix.Watchdog.Feed();
            }

            // was the Game Controller disconnected safety check
            UsbDeviceConnection currentConnectionStatus = js0.GetConnectionStatus();

            if (currentConnectionStatus == UsbDeviceConnection.NotConnected && lastConnectionStatus == UsbDeviceConnection.Connected) {
                switchState(State.Disabled);
            }

            lastConnectionStatus = currentConnectionStatus;
        }

        public void drivePercentage(float speed, float rotation) {
            double[] speeds = arcadeDrive(speed, rotation);
            leftMaster.Set(ControlMode.PercentOutput, speeds[1]);
            //rightMaster.Set(ControlMode.PercentOutput, speeds[0]);
        }





        //////////////////////////////////////
        /// Debug helper functions
        //////////////////////////////////////

        void printGamepadAxes() {
            GameControllerValues values = new GameControllerValues();
            js0.GetAllValues(ref values);
            string output = "";

            for (int i = 0; i < values.axes.Length; ++i) {
                output += values.axes[i] + ",";
            }

            Debug.Print(output);
        }

        void printGamepadButtons() {
            GameControllerValues values = new GameControllerValues();
            js0.GetAllValues(ref values);
            string output = "";

            for (int i = 0; i < 15; ++i) { 
                uint current = values.btns & 0x1;
                values.btns = values.btns >> 1;

                output += current + ",";
            }
            Debug.Print(output);
        }
    }
}
