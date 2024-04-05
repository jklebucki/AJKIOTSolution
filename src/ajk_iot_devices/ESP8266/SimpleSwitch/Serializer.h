// Serializer.h
#ifndef SERIALIZER_H
#define SERIALIZER_H

#include <vector>
#include <sstream>
#include <string>

class DataBlock {
public:
    int int1, int2;
    double double1, double2;

    DataBlock(int i1, int i2, double d1, double d2) : int1(i1), int2(i2), double1(d1), double2(d2) {}
};

class Serializer {
private:
    std::vector<DataBlock> blocks;

public:
    void AddBlock(int int1, int int2, double double1, double double2);
    void RemoveBlock(size_t index);
    std::string Serialize() const;
    void Deserialize(const std::string& data);
    void Print() const;
};

#include "Serializer.cpp"
#endif
