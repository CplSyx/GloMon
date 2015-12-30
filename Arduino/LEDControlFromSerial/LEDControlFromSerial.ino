#include <ArduinoJson.h> 

//Pins for LED colours
#define REDPIN 3
#define GREENPIN 5
#define BLUEPIN 9

#define FADEDELAY 10 //milliseconds to delay between each fade step to smooth the transition of colour

#define DEBUG 1 //enable debug mode functionality

String inData;
String deviceID = "c8933bfa7a074638a61079f1faf44e10";
int fib[] = {1,2,3,5,8,13,21,55,89,144}; //array of Fibonacci we will step through when fading, this gives a gradient of change.
int fibCount = 10;//sizeof(fib) / sizeof(int);

//Values of LED colours. Start with 0.
int currentRed = 0;
int currentGreen = 0;
int currentBlue = 0;

void setup() {

  Serial.begin(115200);

  pinMode(REDPIN, OUTPUT);
  pinMode(GREENPIN, OUTPUT);
  pinMode(BLUEPIN, OUTPUT);

  if(DEBUG)
    pinMode(13, OUTPUT);

  startup();
}

void startup()
{
  setColour(currentRed, currentGreen, currentBlue); //Effectively turns off the LED to start.
  broadcastIdent(); //Send ident to host program

}

void broadcastIdent()
{
  Serial.println("{\"type\":\"IdentPacket\", \"uuid\":\"" + deviceID + "\"}");
}

JsonObject& JSONDecode(String str)
{
  // Step 1: Reserve memory space
  StaticJsonBuffer<300> jsonBuffer;
  
  // Step 2: Convert the "String" into a char array.
  char jsonChars[str.length()];
  str.toCharArray(jsonChars, str.length());

  // Step 3: Deserialize the JSON string
  JsonObject& jObj = jsonBuffer.parseObject(jsonChars);
  
  if (!jObj.success())
  {
    //Return a blank object if we've failed so that validation checks elsewhere will fail correctly rather than error against a null value
    return jsonBuffer.createObject(); 
  }
  //Otherwise return our deserialized object
  return jObj;

}

void setColour(int r, int g, int b)
{
  /* 
   * Loop through the array of Fibonacci numbers, adding or subtracting to reach the target value. 
   * Delay "fadeDelay"ms between each loop to give a smooth fade effect.
   */
  for (int i = 0; i<fibCount; i++)
  {
    if(r > currentRed) //If we need more red
      if((currentRed + fib[i]) > r)
        currentRed = r;
      else
        currentRed = currentRed + fib[i];
    else if(r < currentRed) //If we need less red
      if((currentRed - fib[i]) < r)
        currentRed = r;
      else
        currentRed = currentRed - fib[i];
    //If neither of the above are true then r == currentRed... therefore do nothing.

    if(g > currentGreen) //If we need more red
      if((currentGreen + fib[i]) > g)
        currentGreen = g;
      else
        currentGreen = currentGreen + fib[i];
    else if(g < currentRed) //If we need less green
      if((currentGreen - fib[i]) < g)
        currentGreen = g;
      else
        currentGreen = currentGreen - fib[i];
    //If neither of the above are true then g == currentGreen... therefore do nothing.

    if(b > currentBlue) //If we need more red
      if((currentBlue + fib[i]) > b)
        currentBlue = b;
      else
        currentBlue = currentBlue + fib[i];
    else if(r < currentBlue) //If we need less red
      if((currentBlue - fib[i]) < b)
        currentBlue = b;
      else
        currentBlue = currentBlue - fib[i];
    //If neither of the above are true then b == currentBlue... therefore do nothing.

    //If we are at the limit of values then cap them (as the fib sequence can result in going over or under)
    currentRed = constrain(currentRed, 0, 255);
    currentGreen = constrain(currentGreen, 0, 255);
    currentBlue = constrain(currentBlue, 0, 255);
    
    //Write the values to the pins
    analogWrite(REDPIN, currentRed);
    analogWrite(GREENPIN, currentGreen);
    analogWrite(BLUEPIN, currentBlue);
    
    //Wait - this smooths the transition
    delay(FADEDELAY);
  }
  /*Serial.println("{\"Red\":"+String(r)+",\"Green\":"+String(g)+",\"Blue\":"+String(b)+"}");
  Serial.println("{\"Red\":"+String(currentRed)+",\"Green\":"+String(currentGreen)+",\"Blue\":"+String(currentBlue)+"}");*/
}
void setColour(String r, String g, String b)
{
  setColour(r.toInt(), g.toInt(), b.toInt());
}

void loop() {
  while (Serial.available() > 0)
  {
      char received = Serial.read();      
      // Process message when new line character is recieved
      if (received == '\n')
      {
          inData.trim();
          inData.concat('\0'); //Required to ensure parsing is successful
          JsonObject& root = JSONDecode(inData);
          if (root.containsKey("type") && root["type"].success())
          {  
            String type = root["type"];
            if(type == "RGBData")
            {
              String red = root["red"];
              String green = root["green"];
              String blue = root["blue"];
              
              //Serial.print("Red: "+red+", Green: "+green+", Blue: "+blue+"\n");
              Serial.println("{\"Red\":"+red+",\"Green\":"+green+",\"Blue\":"+blue+"}");
              setColour(red, green, blue);
            }

 
          }
          else
          {
            Serial.println("{\"type\":\"Error\", \"errormessage\":\"Unable to decode.\"}");
            continue;
          }
          
  
          inData = ""; // Clear received buffer
          if(DEBUG)
            digitalWrite(13, !digitalRead(13));
      }
      else
      {
        inData += received; 
      }

  }
}
