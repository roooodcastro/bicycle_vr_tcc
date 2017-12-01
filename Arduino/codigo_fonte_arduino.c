typedef union {
 float number;
 uint8_t bytes[4];
} FloatUnion_t;
 
int potPin = 0;
int hallPin = 2;

volatile unsigned long hallFlipCount = 0;
unsigned long lastHallFlipCount = 0;
unsigned long lastHallTime;
double volatile velocity = 0;
float wheelCircunference = 2.18f; // meters
float hallFlipDistance;
FloatUnion_t steeringAngle;
FloatUnion_t wheelRpm;

double lastVel = 0;
unsigned long lastVelDiffTime = 0;

void setup() {
  Serial.begin(9600);
  pinMode(hallPin, INPUT_PULLUP);
  hallFlipDistance = wheelCircunference / 16.0f;
  steeringAngle.number = 0;
  wheelRpm.number = 0;
  lastHallTime = micros();
  attachInterrupt(0, pin_ISR, CHANGE);
}

void loop() {
  int potValue = analogRead(potPin);
  steeringAngle.number = (potValue - 512) / 4.0f;
  //Serial.println(angle);
  checkForZeroVelocity();
  answerPing();
  delay(1);
}

void updateVelocity() {
  unsigned long currentTime = micros();
  unsigned long timeDiff = currentTime - lastHallTime;
  float minutes = timeDiff / 60000000.0f;
  lastHallTime = currentTime;
  
  double newVelocity = (hallFlipDistance / timeDiff) * 1000000 * 3.6f;
  velocity = newVelocity * 0.1f + velocity * 0.9f;

  // 0.0625 is 1/16 of a rotation, since there's 16 magnets in the wheel.
  double newRpm = 0.0625f / minutes;
  wheelRpm.number = newRpm * 0.5f + wheelRpm.number * 0.5f;
}

void checkForZeroVelocity() {
  if (velocity == lastVel) {
    unsigned long timeDiff = millis() - lastVelDiffTime;
    if (timeDiff > 1000) {
      velocity = 0;
      wheelRpm.number = 0;
    }
  } else {
    lastVelDiffTime = millis();
    lastVel = velocity;
  }
}

void pin_ISR() {
  hallFlipCount++;
  updateVelocity();
}

void answerPing() {
  if (Serial.available() > 0) {
    char command = Serial.read();
    if (command == 'a') {
      Serial.write(steeringAngle.bytes, 4);
    } else if (command == 'b') {
      Serial.write(wheelRpm.bytes, 4);
    }
  }
}
