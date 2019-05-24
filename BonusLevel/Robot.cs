using System;
using Microsoft.SPOT;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix;

namespace BonusLevel {
    public class Robot {
        public enum State { Enabled, Disabled, None };

        GameController js0 = null;

        private State currentState = State.None, newState = State.Disabled;

        private void robotInit() {
            js0 = new GameController(UsbHostDevice.GetInstance());
        }

        private void enabledInit() {

        }

        private void disabledInit() {

        }

        private void robotPeriodic() {
            printGamepadButtons();
        }

        private void enabledPeriodic() {

        }

        private void disabledPeriodic() {

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
            // Keeps talonsrx enabled
            CTRE.Phoenix.Watchdog.Feed();

            // was the Game Controller disconnected safety check
            UsbDeviceConnection currentConnectionStatus = js0.GetConnectionStatus();

            if (currentConnectionStatus == UsbDeviceConnection.NotConnected && lastConnectionStatus == UsbDeviceConnection.Connected) {
                switchState(State.Disabled);
            }

            lastConnectionStatus = currentConnectionStatus;
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
