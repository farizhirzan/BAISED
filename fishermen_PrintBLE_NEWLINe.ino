#include "Adafruit_Thermal.h"
#include <SoftwareSerial.h>
#include <Arduino.h>
#include <SPI.h>
#include "Adafruit_BLE.h"
#include "Adafruit_BluefruitLE_SPI.h"
#include "Adafruit_BluefruitLE_UART.h"
#include "BluefruitConfig.h"
#define TX_PIN 6 // Arduino transmit  YELLOW WIRE  labeled RX on printer
#define RX_PIN 5 // Arduino receive   GREEN WIRE   labeled TX on printer
#define FACTORYRESET_ENABLE 1
#define MINIMUM_FIRMWARE_VERSION "0.6.6"
#define MODE_LED_BEHAVIOUR "MODE"

SoftwareSerial mySerial(RX_PIN, TX_PIN); // Declare SoftwareSerial obj first
Adafruit_Thermal printer(&mySerial);// Pass addr to printer constructor
Adafruit_BluefruitLE_SPI ble(BLUEFRUIT_SPI_CS, BLUEFRUIT_SPI_IRQ, BLUEFRUIT_SPI_RST);

// A small helper
void error(const __FlashStringHelper*err) {
 Serial.println(err);
 while (1);
}

void setup(void)
{
 // while (!Serial);  // required for Flora & Micro
  delay(500);
  mySerial.begin(9600);  // Initialize SoftwareSerial
  //Serial1.begin(19200); // Use this instead if using hardware serial
  printer.begin();
  Serial.begin(115200);
  Serial.println(F("Adafruit Bluefruit Command <-> Data Mode Example"));
  Serial.println(F("------------------------------------------------"));
  /* Initialise the module */
  Serial.print(F("Initialising the Bluefruit LE module: "));
  if ( !ble.begin(VERBOSE_MODE) )
  {
      error(F("Couldn't find Bluefruit, make sure it's in CoMmanD mode & check wiring?"));
  }
  Serial.println( F("OK!") );
  if ( FACTORYRESET_ENABLE )
  {
  /* Perform a factory reset to make sure everything is in a known state */
    Serial.println(F("Performing a factory reset: "));
    if ( ! ble.factoryReset() ){
    error(F("Couldn't factory reset"));
  }

  }
  /* Disable command echo from Bluefruit */
  ble.echo(false);
  Serial.println("Requesting Bluefruit info:");
  /* Print Bluefruit information */
  ble.info();
  //Serial.println(F("Please use Adafruit Bluefruit LE app to connect in UART mode"));
  //Serial.println(F("Then Enter characters to send to Bluefruit"));
  // Serial.println();
  ble.verbose(false);  // debug info is a little annoying after this point!
  /* Wait for connection */

  while (! ble.isConnected()) {
   delay(500);
  }
  Serial.println(F("******************************"));
  // LED Activity command is only supported from 0.6.6
  if ( ble.isVersionAtLeast(MINIMUM_FIRMWARE_VERSION) )
  {
    // Change Mode LED Activity
    Serial.println(F("Change LED activity to " MODE_LED_BEHAVIOUR));
    ble.sendCommandCheckOK("AT+HWModeLED=" MODE_LED_BEHAVIOUR);
  }
  // Set module to DATA mode
  Serial.println( F("Switching to DATA mode!") );
  ble.setMode(BLUEFRUIT_MODE_DATA);
  Serial.println(F("******************************"));
  printer.justify('C');
}

void loop(void){
  // Check for user input
 
  char n, inputs[BUFSIZE+1];
 if (Serial.available()){
  n = Serial.readBytes(inputs, BUFSIZE);
  inputs[n] = 0;
  // Send characters to Bluefruit
  Serial.print("Sending: ");
  Serial.println(inputs);
  // Send input data to host via Bluefruit
  ble.print(inputs);
  }

String message = ""; // Declare a string to hold your message

while (ble.available()) {
  char c = (char)ble.read(); // Read a character

  if (c == '\n' || c == '\r') { // Check if it's a newline or carriage return
    // You've got a full message, print it
    Serial.println(message);
    String text = message;
     printer.feed(2);
     printer.print("  ");
    // Print message to thermal printer
   int textLen = text.length();   
   int lineStart = 0;   
   for (int i = 0; i < textLen; i++) {     
       if (i - lineStart >= 27) { // If we have exceeded the line length     
          if (text[i] == ' ') { // If current character is space        
              printer.println(text.substring(lineStart, i)); // Print the line        
              lineStart = i + 1; // Set the start of the next line      
          } else {         
            int lastSpace = text.lastIndexOf(' ', i); // Find the last space before current position        
            if (lastSpace > lineStart) { // If a space was found          
                printer.println(text.substring(lineStart, lastSpace)); // Print the line          
                lineStart = lastSpace + 1; // Set the start of the next line        
            }       
          }     
      } else if (i == textLen - 1) { // If we reached the end of the string      
              printer.println(text.substring(lineStart, i + 1)); // Print the remaining line    
     }   
   } 
  
    printer.feed(2);

    // Clear the message string to get ready for the next one
    message = "";
}else {
    // Not a newline, so this character is part of the message
    message += c;
  }
 }
}

