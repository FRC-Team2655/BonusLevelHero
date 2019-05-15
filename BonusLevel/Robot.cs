using System;
using Microsoft.SPOT;

namespace BonusLevel {
    public class Robot {
        public enum State { Enabled, Disabled, None };

        private State currentState = State.None, newState = State.Disabled;

        private void robotInit() {

        }

        private void enabledInit() {

        }

        private void disabledInit() {

        }

        private void robotPeriodic() {

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

            robotPeriodic();
        }
    }
}
