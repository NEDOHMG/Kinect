#include <Arduino.h>
#include "BluetoothSerial.h"

// Create one object of the BluetoothSerialClass
BluetoothSerial SerialBT;

int PGM_ValveA = 25;
int PGM_ValveB = 33;
int PGM_ValveC = 32;

int LED = 26;

int w = 0;

 
void setup() {
  Serial.begin(115200);
 
  pinMode(PGM_ValveA, OUTPUT);
  pinMode(PGM_ValveB, OUTPUT);
  pinMode(PGM_ValveC, OUTPUT);
  pinMode(LED, OUTPUT);

  // This will stat the Bluetooth 
  SerialBT.begin("ESP32NEDO");

}
 
void loop() {
 
  SerialBT.read();
  w = SerialBT.parseInt();

  if( w == 1) {
    LED = false;
  } else if (w == 0){
    LED = true;
  }

  if(LED)
    {
      digitalWrite(PGM_ValveA, HIGH);
      digitalWrite(PGM_ValveB, HIGH);
      digitalWrite(PGM_ValveC, HIGH);
    } else {
      digitalWrite(PGM_ValveA, LOW);
      digitalWrite(PGM_ValveB, LOW);
      digitalWrite(PGM_ValveC, LOW);
    }
  
  // clean the variable 
  w = 0;

}
