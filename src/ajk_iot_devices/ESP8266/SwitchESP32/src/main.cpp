#include <Arduino.h>

// put function declarations here:
int myFunction(int, int);

void setup()
{
  Serial.begin(115200);
}

// put function definitions here:
int myFunction(int x, int y)
{
  return x + y;
}

void loop()
{
  Serial.println("Hello World!");
  Serial.print("Sum 2+3 is: ");
  Serial.println(myFunction(2, 3));
  delay(1000);
}