/*
  RaceController - DumboRC P6DC(G) PWM receiver to USB serial

  Wiring:
    CH1 signal -> Arduino Uno D2
    CH2 signal -> Arduino Uno D3
    Receiver GND -> Arduino GND

  Serial protocol at 115200 baud:
    RC,seq,ch1_us,ch2_us,status
*/

const byte CH1_PIN = 2;
const byte CH2_PIN = 3;

const unsigned long BAUD_RATE = 115200;
const unsigned long SAMPLE_PERIOD_US = 20000;
const unsigned long SIGNAL_TIMEOUT_US = 100000;

const uint16_t MIN_VALID_PULSE_US = 800;
const uint16_t MAX_VALID_PULSE_US = 2200;

volatile unsigned long ch1RiseUs = 0;
volatile unsigned long ch2RiseUs = 0;
volatile uint16_t ch1PulseUs = 1500;
volatile uint16_t ch2PulseUs = 1500;
volatile unsigned long ch1LastPulseUs = 0;
volatile unsigned long ch2LastPulseUs = 0;

unsigned long lastSampleUs = 0;
uint32_t sequence = 0;

void setup() {
  pinMode(CH1_PIN, INPUT);
  pinMode(CH2_PIN, INPUT);

  Serial.begin(BAUD_RATE);

  attachInterrupt(digitalPinToInterrupt(CH1_PIN), onCh1Change, CHANGE);
  attachInterrupt(digitalPinToInterrupt(CH2_PIN), onCh2Change, CHANGE);
}

void loop() {
  const unsigned long now = micros();
  if (now - lastSampleUs < SAMPLE_PERIOD_US) {
    return;
  }
  lastSampleUs = now;

  uint16_t ch1;
  uint16_t ch2;
  unsigned long ch1Age;
  unsigned long ch2Age;

  noInterrupts();
  ch1 = ch1PulseUs;
  ch2 = ch2PulseUs;
  ch1Age = now - ch1LastPulseUs;
  ch2Age = now - ch2LastPulseUs;
  interrupts();

  const bool ch1TimedOut = ch1Age > SIGNAL_TIMEOUT_US;
  const bool ch2TimedOut = ch2Age > SIGNAL_TIMEOUT_US;

  Serial.print(F("RC,"));
  Serial.print(sequence++);
  Serial.print(',');
  Serial.print(ch1);
  Serial.print(',');
  Serial.print(ch2);
  Serial.print(',');

  if (ch1TimedOut && ch2TimedOut) {
    Serial.println(F("CH1_CH2_TIMEOUT"));
  } else if (ch1TimedOut) {
    Serial.println(F("CH1_TIMEOUT"));
  } else if (ch2TimedOut) {
    Serial.println(F("CH2_TIMEOUT"));
  } else {
    Serial.println(F("OK"));
  }
}

void onCh1Change() {
  capturePulse(CH1_PIN, ch1RiseUs, ch1PulseUs, ch1LastPulseUs);
}

void onCh2Change() {
  capturePulse(CH2_PIN, ch2RiseUs, ch2PulseUs, ch2LastPulseUs);
}

void capturePulse(
  const byte pin,
  volatile unsigned long &riseUs,
  volatile uint16_t &pulseUs,
  volatile unsigned long &lastPulseUs
) {
  const unsigned long now = micros();

  if (digitalRead(pin) == HIGH) {
    riseUs = now;
    return;
  }

  const unsigned long width = now - riseUs;
  if (width >= MIN_VALID_PULSE_US && width <= MAX_VALID_PULSE_US) {
    pulseUs = (uint16_t)width;
    lastPulseUs = now;
  }
}
