
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using UnityEngine;

public class FieldClass : MonoBehaviour
{
    public FieldClass(float TimerLength, int XSize, int YSize, float UnitCellSize)
    {

    }

    public float FieldTimer;

    private float FieldTimerLength;

    public void TryUpdate()
    {
        FieldTimer = FieldTimerLength;

    }

    [Flags]
    public enum FieldProperties
    {
        UnitMultiplication = 1,

        ParticlesPerUnit_2 = 2,
        ParticlesPerUnit_4 = 4,
        ParticlesPerUnit_8 = 8,
        ParticlesPerUnit_12 = 16,
        ParticlesPerUnit_16 = 32,

        UnitState_N2 = 64,
        UnitState_N1p5 = 128,
        UnitState_N1 = 256,
        UnitState_N0p5 = 512,
        UnitState_0 = 1024,
        UnitState_0p5 = 2048,
        UnitState_1 = 4096,
        UnitState_1p5 = 8192,
        UnitState_2 = 16384,

        UnitSum = 32768,
    }

    int XSize;
    int YSize;
    float UnitCellSize;

    FieldUnitClass[,] FieldSpace;

    public FieldUnitClass GetUnit(int X, int Y)
    {
        return FieldSpace[X, Y];
    }

    //-----------------------Vector Field---------------------//

    Vector3 BasalDeformationState;

    public void SetUpAsVectorField(Vector3 BasalDeformationValue)
    {
        BasalDeformationState = BasalDeformationValue;
        BasalVectorState = Vector3.zero;
        DeformationMatrix = new Vector3[VectorFieldXSize, VectorFieldYSize];
        VectorField = new Vector3[VectorFieldXSize, VectorFieldYSize];
        FieldChangeMatrix = new int[VectorFieldXSize, VectorFieldYSize];

        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                DeformationMatrix[i, j] = BasalVectorState;
                VectorField[i, j] = BasalVectorState;
                FieldChangeMatrix[i, j] = 0;
            }
        }
    }

    public enum VectorFieldType
    {
        ForceField, DeformationField, VedtorField
    }

    [SerializeField]
    int VectorFieldXSize, VectorFieldYSize;

    [SerializeField]
    float MaxVectorValue, MinVectorValue;

    public void SetMinMaxVectorValues(float maxValue, float minValue)
    {
        MaxVectorValue = maxValue;
        MinVectorValue = minValue;
    }

    public float GetVectorSpaceMinMaxDelta()
    {
        return MaxVectorValue - MinVectorValue;
    }

    public void GetSize(out int X, out int Y)
    {
        X = VectorFieldXSize; Y = VectorFieldYSize;
    }

    int[,] FieldChangeMatrix; // 0 =  no computation required ,1 = Was Updated Last Frame Normalize With Surroundings, 2 = Compute 

    Vector3[,] VectorField;

    Vector3[,] DeformationMatrix;

    [SerializeField]
    AnimationCurve DeformationCurve;
    [SerializeField]
    AnimationCurve AbsorptionRate;

    public Vector3 GetVectorAtPosition(int i, int j)
    {
        return VectorField[i, j];
    }

    public void SetPulseAtPosition(int i , int j,Vector3 Pulse)
    {
        if(i < VectorFieldXSize && j < VectorFieldYSize && i >= 0 && j >= 0)
        {
            Debug.Log("FieldChangeMatrix[" + i + "," + j + "] ==" + (counter + 1));
            FieldChangeMatrix[i, j] = counter+1; 
            VectorField[i,j] = Pulse;
          
        }
    }
    public void SetPulseAtPosition(int i, int j, Vector3 Pulse, PulsePropagationMode Mode)
    {
        if (i < VectorFieldXSize && j < VectorFieldYSize && i >= 0 && j >= 0)
        {
            Debug.Log("FieldChangeMatrix[" + i + "," + j + "] ==" + (counter + 1));
            FieldChangeMatrix[i, j] = counter+1;
            VectorField[i, j] = Pulse;
        }
    }

    public enum PulsePropagationMode
    {
        Lerp,
        TrigonometricLerp, //(   (V1.x * cos(a) + V2.x ) / 2  , (V1.y * sin(a) + V2.y ) / 2  )
    }

    int counter = 0;

    private Vector3 VectorOperation(Vector3 V1,Vector3 V2,PulsePropagationMode Mode)
    {
        Vector3 vec;
        switch (Mode)
        {
            case PulsePropagationMode.Lerp:
               vec =  Vector3.Lerp(V1, V2, 0.5f);
                break;
            case PulsePropagationMode.TrigonometricLerp:
                float a = Vector2.Angle(V1, V2);
                Debug.Log("Angle a = " + a);
                float az  = Vector2.Angle(Vector2.zero, new Vector2(V2.z, V2.y));
                vec =  new Vector3((Mathf.Cos(a) * V2.x) + V1.x,(Mathf.Sin(a) * V2.y) + V1.y, (Mathf.Cos(az) * V2.z)  + V1.z);
                break;
            default:
                vec = Vector3.zero;
                break;
        }

        return new Vector3 (
            Mathf.Clamp(vec.x,-MaxVectorValue,MaxVectorValue), 
            Mathf.Clamp(vec.y, -MaxVectorValue, MaxVectorValue),
            Mathf.Clamp(vec.z, -MaxVectorValue, MaxVectorValue)
            );
    }

    public void PropagatePulse(PulsePropagationMode Mode)
    {
        Debug.Log("PropagatePulse");
        counter++;
        for (int i = 1; i < VectorFieldXSize - 1; i++)
        {
            for (int j = 1; j < VectorFieldYSize - 1; j++)
            {
                if (FieldChangeMatrix[i,j] == counter)
                {
                    Debug.Log("FieldChangeMatrix["+i+","+j+"] == " + counter);
                    if (FieldChangeMatrix[i + 1, j] == 0 || FieldChangeMatrix[i + 1, j] < counter - 1)
                    {
                        FieldChangeMatrix[i + 1, j] = counter+1;
                        VectorField[i + 1, j] = VectorOperation(VectorField[i + 1, j], VectorField[i , j], Mode);
                    }
                    if (FieldChangeMatrix[i - 1, j] == 0 || FieldChangeMatrix[i - 1, j] < counter - 1)
                    {
                        FieldChangeMatrix[i - 1, j] = counter + 1;
                        VectorField[i - 1, j] = VectorOperation(VectorField[i - 1, j], VectorField[i, j], Mode);                     
                    }
                    if (FieldChangeMatrix[i , j + 1] == 0 || FieldChangeMatrix[i, j + 1] < counter - 1)
                    {
                        FieldChangeMatrix[i , j + 1] = counter + 1;
                        VectorField[i, j + 1] = VectorOperation(VectorField[i, j + 1], VectorField[i, j], Mode);                       
                    }
                    if (FieldChangeMatrix[i , j - 1] == 0 || FieldChangeMatrix[i, j - 1] < counter - 1)
                    {
                        FieldChangeMatrix[i, j - 1] = counter + 1;
                        VectorField[i, j - 1] = VectorOperation(VectorField[i, j - 1], VectorField[i, j], Mode);                     
                    }
                }
             }
        }
    }

    //---------------Basal State Operations-------------//
    
    Vector3 BasalVectorState;

    public void SetBasalVectorState(Vector3 state)
    {
        BasalVectorState = state;
    }

    public void SetDeformationMatrixToBasal()
    {
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                DeformationMatrix[i, j]
                    = BasalDeformationState;
            }
        }
    }

    public void SetToBasal()
    {
        Debug.Log("SetToBasal()");
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                VectorField[i, j]
                    = BasalVectorState;
            }
        }
    }
   
    //----------------Field Vector Operations-----------------//

    public void MultiplyFieldByVector(Vector3 vec)
    {
        Debug.Log("MultiplyFieldByVector()");
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                VectorField[i, j]
                   = new Vector3(
                     Mathf.Clamp(VectorField[i, j].x * vec.x, MinVectorValue, MaxVectorValue),
                     Mathf.Clamp(VectorField[i, j].y * vec.y, MinVectorValue, MaxVectorValue),
                     Mathf.Clamp(VectorField[i, j].z * vec.z, MinVectorValue, MaxVectorValue)
                     );
            }
        }
    }

    public void SumFieldByVector(Vector3 vec)
    {
        Debug.Log("SumFieldByVector()");
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                VectorField[i, j]
                  = new Vector3(
                    Mathf.Clamp(VectorField[i, j].x + vec.x, MinVectorValue, MaxVectorValue),
                    Mathf.Clamp(VectorField[i, j].y + vec.y, MinVectorValue, MaxVectorValue),
                    Mathf.Clamp(VectorField[i, j].z + vec.z, MinVectorValue, MaxVectorValue)
                    );
            }
        }
    }

    public void CrossFieldByVector(Vector3 vec)
    {
        Debug.Log("SumFieldByVector()");
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                VectorField[i, j]
                  = new Vector3(
                    Mathf.Clamp( VectorField[i, j].y * vec.z - VectorField[i, j].z * vec.y  , MinVectorValue, MaxVectorValue ),
                    Mathf.Clamp( VectorField[i, j].z * vec.x - VectorField[i, j].x * vec.z, MinVectorValue, MaxVectorValue ),
                    Mathf.Clamp( VectorField[i, j].x * vec.y - VectorField[i, j].y * vec.x, MinVectorValue, MaxVectorValue )
                    );
            }
        }
    }

    public void SetVectorsToRandom()
    {
        Debug.Log("SetVectorsToRandom()");
        for (int i = 0; i < VectorFieldXSize; i++)
        {
            for (int j = 0; j < VectorFieldYSize; j++)
            {
                VectorField[i, j] 
                    = new Vector3(
                      UnityEngine.Random.Range(MinVectorValue, MaxVectorValue)
                    , UnityEngine.Random.Range(MinVectorValue, MaxVectorValue)
                    , UnityEngine.Random.Range(MinVectorValue, MaxVectorValue) 
                    );
            }
        }
  
    }

}
