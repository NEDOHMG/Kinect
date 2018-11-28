#include <Arduino.h>
#include "BluetoothSerial.h"
 
BluetoothSerial SerialBT;

int ledPin = 25;
boolean LED = false;
int w = 0;

 
void setup() {
  Serial.begin(115200);
 
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW);

  SerialBT.begin("ESP32");

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
      digitalWrite(ledPin, HIGH);
    } else {
      digitalWrite(ledPin, LOW);
    }
  
  // clean the variable 
  w = 0;

}
