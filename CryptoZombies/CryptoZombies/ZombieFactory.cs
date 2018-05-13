using Stratis.SmartContracts;
using System;
using System.Text;

public class ZombieFactory : SmartContract
{
    private uint dnaDigits;
    private uint dnaModulus;

    public ISmartContractList<string> ZombieNames
    {
        get
        {
            return PersistentState.GetList<string>("ZombieNames");
        }
    }

    public ISmartContractList<uint> ZombieDNAs
    {
        get
        {
            return PersistentState.GetList<uint>("ZombieDNAs");
        }
    }

    public ZombieFactory(ISmartContractState smartContractState) : base(smartContractState)
    {
        this.dnaDigits = 16;
        this.dnaModulus = Pow(10, dnaDigits);
    }

    public uint CreateRandomZombie(string name)
    {
        uint randomData = GenerateRandomDna(name);

        CreateZombie(name, randomData);

        return randomData;
    }

    private void CreateZombie(string name, uint dna)
    {
        ZombieNames.Add(name);
        ZombieDNAs.Add(dna);
    }

    private uint GenerateRandomDna(string value)
    {
        byte[] rand = Keccak256(ToByteArray(value));

        // We want our DNA to only be 16 digits long
        return ToUInt32(rand) % this.dnaModulus;
    }

    private uint Pow(uint x, uint y)
    {
        if (y == 0) return 1;

        return x * Pow(x, y - 1);
    }

    private byte[] ToByteArray(string value)
    {
        char[] charArr = value.ToCharArray();

        byte[] bytes = new byte[charArr.Length];

        for(int i = 0; i < charArr.Length; i++)
        {
            byte current = Convert.ToByte(charArr[i]);
            bytes[i] = current;
        }

        return bytes;
    }

    private uint ToUInt32(byte[] data)
    {
        var requiredSize = 4;

        Assert(data.Length != requiredSize);

        var result = 0u;

        for(var i = 0; i < requiredSize; i++)
        {
            result |= ((uint)data[i] << ((requiredSize - (i + 1)) * 8));
        }

        return result;
    }
}


