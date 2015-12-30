#include <ArduinoJson.h>

String inData;
String deviceID = "c8933bfa7a074638a61079f1faf44e10";

void setup() {
  // put your setup code here, to run once:
  Serial.begin(115200);
  pinMode(13, OUTPUT);
  broadcastIdent();
  
}

void broadcastIdent()
{
  Serial.print("{\"type\":\"IdentPacket\", \"uuid\":\"" + deviceID + "\"}\n");
}

JsonObject& JSONDecode(String str)
{
  //Decode JSON
  //
  // Step 1: Reserve memory space
  //
  StaticJsonBuffer<300> jsonBuffer;
  
  //
  // Step 2: Deserialize the JSON string
  //
  
  char jsonChars[str.length()];
  str.toCharArray(jsonChars, str.length());
  JsonObject& jObj = jsonBuffer.parseObject(jsonChars);
  
  if (!jObj.success())
  {
    return jsonBuffer.createObject(); //return a blank object if we've failed so that validation checks elsewhere will fail correctly rather than error
  }
  return jObj;

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
            }

 
          }
          else
          {
            //Serial.println("{\"type\":\"Error\", \"errormessage\":\"Unable to decode.\"}");
            continue;
          }
          
  
          inData = ""; // Clear received buffer
          digitalWrite(13, !digitalRead(13));
      }
      else
      {
        inData += received; 
      }

  }
}
