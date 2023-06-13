
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class SystemMain : MonoBehaviour 
{

    public static SystemMain instance;

    [SerializeField]
    Transform RefPos;
    [SerializeField]
    RectTransform Content;
    FieldClass[] Fields;

    [SerializeField]
    int NumberOfFields;

    [SerializeField]
    GameObject FieldUnitObj;

    private void Awake()
    {
        instance = this;
        a = Color.cyan;
        b = Color.red;
        c = Color.blue;
        d = Color.red;

    }

    private void Start()
    {


        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
     
        


      


    }
    //----------------------------Vector Field Display
    private void GenerateSystem()
    {
        


        Fields = new FieldClass[NumberOfFields];



        for (int i = 0; i < Fields.Length; i++)
        {
            Fields[i] = new FieldClass(0.5f,16,16,8);
            for (int j = 0; j < 16; j++)
            {
                for (int k = 0; k < 16; k++)
                {
                    GameObject Unit = Instantiate(FieldUnitObj, new Vector3(RefPos.position.x + j*8 +4, RefPos.position.y + k * 8 +4, RefPos.position.z),Quaternion.identity);  
                }
            }
        }
        TransformDataArray = new VectorFieldTransformData[FieldsArray.Length];
        FieldParentObjectsArray = new GameObject[FieldsArray.Length];
        for (int i = 0; i < FieldsArray.Length; i++)
        {
            GameObject Obj = new GameObject();
            FieldParentObjectsArray[i] = Obj;
            FieldParentObjectsArray[i].transform.position = RefPos.position;
        }

        FieldsArray[0].SetUpAsVectorField(Vector3.zero);

        CreateVectorField(FieldsArray[0], 0);

        FieldsArray[0].SetVectorsToRandom();

        UpdateVectorField(0);

        UpdateCustomVector();
    }

    private void UpdateFields()
    {
        for (int i = 0; i < Fields.Length; i++)
        {
            Fields[i].FieldTimer -= Time.deltaTime;
            if (Fields[i].FieldTimer < 0) { Fields[i].TryUpdate(); }
        }
    }



    [SerializeField]
    FieldClass[] FieldsArray;

    VectorFieldTransformData[] TransformDataArray;

    GameObject[] FieldParentObjectsArray;

    public struct VectorFieldTransformData
    {
        public Transform[,] VectorUnitTransforms;
        public int FieldID;
        public VectorFieldTransformData(Transform[,] data,int ID)
        {
            VectorUnitTransforms = data;
            FieldID = ID;
        }
    }

    [SerializeField]
    GameObject VectorUnitObj;
    [SerializeField]
    float CellSize;

    private void CreateVectorField(FieldClass field,int FieldID)
    {
        int X = 0;
        int Y = 0;
        field.GetSize(out X, out Y);

        Transform[,] transformArray = new Transform[X, Y];

        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
            {
                GameObject Obj = Instantiate(VectorUnitObj,
                    new Vector3
                     ( 
                       RefPos.transform.position.x - (X * CellSize / 2) + i * CellSize + (CellSize / 2),
                       RefPos.transform.position.y - (Y * CellSize / 2) + j * CellSize + (CellSize / 2),
                       RefPos.transform.position.z
                     ),
                    Quaternion.identity);
                transformArray[i, j] = Obj.transform;
                Obj.transform.SetParent(FieldParentObjectsArray[FieldID].transform);
            }
        }

        TransformDataArray[FieldID] = new VectorFieldTransformData(transformArray, FieldID);
    }

    [SerializeField]

    float ScaleMod;

    [SerializeField]
    float UpdateTimer;

    public void UpdateVectorField(int FieldID)
    {
        int X = 0;
        int Y = 0;

        float delta = FieldsArray[FieldID].GetVectorSpaceMinMaxDelta();

        FieldsArray[FieldID].GetSize(out X, out Y);

        for (int i = 0; i < X; i++)
        {
            for (int j = 0; j < Y; j++)
            {
              //  Debug.Log(FieldsArray[FieldID].GetVectorAtPosition(i, j));
                TransformDataArray[FieldID].VectorUnitTransforms[i,j].localScale 
                    = new Vector3(FieldsArray[FieldID].GetVectorAtPosition(i,j).magnitude/16 * ScaleMod, FieldsArray[FieldID].GetVectorAtPosition(i, j).magnitude / 64 * ScaleMod, 1) ;

               // Debug.Log("field.GetVectorAtPosition(" + i + "," + j + ").magnitude = " + FieldsArray[FieldID].GetVectorAtPosition(i, j).magnitude);
               // Debug.Log(FieldsArray[FieldID].GetVectorAtPosition(i, j));
               
                Quaternion rot = Quaternion.Euler(
                    0,
                    0,
                    Mathf.Atan2(FieldsArray[FieldID].GetVectorAtPosition(i, j).y, FieldsArray[FieldID].GetVectorAtPosition(i, j).x) * Mathf.Rad2Deg
                    );

                TransformDataArray[FieldID].VectorUnitTransforms[i, j].rotation = rot;

                TransformDataArray[FieldID].VectorUnitTransforms[i, j]
                    .GetComponent<SpriteRenderer>().color
                    = Color.Lerp(Color.red, Color.blue, FieldsArray[FieldID].GetVectorAtPosition(i, j).z/delta + 0.5f);
            }
        }


    }

    float CustomVectorX;public void ChangeCVX(float delta) { CustomVectorX += delta; Mathf.Clamp(CustomVectorX, 16, 16);UpdateCustomVector(); }
    float CustomVectorY; public void ChangeCVY(float delta) { CustomVectorY += delta; Mathf.Clamp(CustomVectorY, 16, 16); UpdateCustomVector(); }
    float CustomVectorZ; public void ChangeCVZ(float delta) { CustomVectorZ += delta; Mathf.Clamp(CustomVectorZ, 16, 16); UpdateCustomVector(); }
     
    [SerializeField]
    Text[] CustomVectorUIText;

    private void UpdateCustomVector()
    {
        if(CustomVectorX < 0.1f && CustomVectorX > -0.1f) { CustomVectorX = 0; }
        if (CustomVectorY < 0.1f && CustomVectorY > -0.1f) { CustomVectorY = 0; }
        if (CustomVectorZ < 0.1f && CustomVectorZ > -0.1f) { CustomVectorZ = 0; }
        CustomVectorUIText[0].text = CustomVectorX.ToString();
        CustomVectorUIText[1].text = CustomVectorY.ToString();
        CustomVectorUIText[2].text = CustomVectorZ.ToString();
    }

    public void ReGenerateField()
    {
        FieldsArray[0].SetVectorsToRandom();

        UpdateVectorField(0);

    }
    public void SetToBasal()
    {
        FieldsArray[0].SetToBasal();

        UpdateVectorField(0);

    }
    public void MultipyVectorField()
    {
        FieldsArray[0].MultiplyFieldByVector(new Vector3(CustomVectorX, CustomVectorY, CustomVectorZ));

        UpdateVectorField(0);

    } 
    public void SumVectorField()
    {
        FieldsArray[0].SumFieldByVector(new Vector3(CustomVectorX, CustomVectorY, CustomVectorZ));

        UpdateVectorField(0);

    }
    public void CrossProdVectorField()
    {
        FieldsArray[0].CrossFieldByVector(new Vector3(CustomVectorX, CustomVectorY, CustomVectorZ));

        UpdateVectorField(0);

    }

    public void SelectSlotXY(int X,int Y)
    {



    }
    // testing
    public void PropagatePulse()
    {
        FieldsArray[0].PropagatePulse(FieldClass.PulsePropagationMode.TrigonometricLerp); 
        UpdateVectorField(0);
    }
    public void SetNewPulse()
    {
        Debug.Log("SetNewPulse()");
        FieldsArray[0].GetSize(out int A,out int B);
        FieldsArray[0].SetPulseAtPosition(Random.Range(1,A), Random.Range(1, B), new Vector3(CustomVectorX, CustomVectorY, CustomVectorZ));
        UpdateVectorField(0);
    }

    private Vector3 Dfn(Vector3 Df0, Vector3 a)
    {

        return Vector3.zero;
    }
    private float ResistanceCoefeicient(Vector3 Force)
    {

        return 0;
    }
    private Vector3 ShiftVector(Vector3 V0, Vector3 V1, Vector3 V2, Vector3 V3)
    {
        return Vector3.zero;
    }
    //------------------------------------UI Canvas-----------------------------//

    [SerializeField]
    GameObject MainAreaCanvas; 
    public void SetActiveMainAreaCanvas() 
    { 
        MainAreaCanvas.SetActive(true);

        FieldViewPanelCanvas.SetActive(false);
        FieldViewCanvas.SetActive(false);

        UnitViewModeCanvas.SetActive(false);

        mesh.Clear();
    }

    [SerializeField]
    GameObject FieldViewCanvas,FieldViewPanelCanvas;
    public void SetActiveFieldCanvas() 
    {
        FieldViewPanelCanvas.SetActive(true);
        FieldViewCanvas.SetActive(true);

        MainAreaCanvas.SetActive(false);
        UnitViewModeCanvas.SetActive(false);
        GenerateSystem();
        mesh.Clear();
    }

    [SerializeField]
    GameObject UnitViewModeCanvas;
    public void SetActiveUnitViewModeCanvas() 
    { 
        Debug.Log("SetActiveUnitViewModeCanvas");
        UnitViewModeCanvas.SetActive(true); 

        MainAreaCanvas.SetActive(false);

        FieldViewPanelCanvas.SetActive(false);
        FieldViewCanvas.SetActive(false);
        viewMode = ViewMode.MultiQuad;
        switch (viewMode)
        {
            case ViewMode.SingleQuad:
                SetDeformationMesh(); UpdateDeformationMesh(); Debug.Log("A");
                break;
            case ViewMode.MultiQuad:
                SetMultiQuadMeshTriangles(); UpdateMultiQuadMesh(); Debug.Log("B");
                break;
        }

    }

    //------------------UnitVIew-------------------//

    [SerializeField]
    GameObject SingelQuad;
    [SerializeField]
    GameObject MultiQuad;

    float BasalDistance = 16;

    [SerializeField]
    Slider AngleSlider;
    [SerializeField]
    Slider ForceSlider;

    public enum ViewMode
    {
        SingleQuad,MultiQuad
    }
    public enum ForcePropagationModels
    {
        Model1,
        Model2,
        Model3
    }

    ForcePropagationModels FPM = ForcePropagationModels.Model1;

    ViewMode viewMode= ViewMode.MultiQuad;

    [SerializeField]
    Transform Vertex00; //UsedBy MultiQuadSystem  

    [SerializeField]
    Transform Vertex01;
    [SerializeField]
    Transform Vertex10;
    [SerializeField]
    Transform Vertex11;

    float ForceAngle;
    float ForceMagnitude;

    [SerializeField]
    Text Df00Data;
    [SerializeField]
    Text Df01Data;
    [SerializeField]
    Text Df10Data;
    [SerializeField]
    Text Df11Data;
    [SerializeField]
    Text Vertex00Data;
    [SerializeField]
    Text Vertex01Data;
    [SerializeField]
    Text Vertex10Data;
    [SerializeField]
    Text Vertex11Data;
    [SerializeField]
    Text VectorForceDisplay;
    [SerializeField]
    Text VectorAngleDisplay;

    //---------SingleQaudMeshData------//

       Vector3
     V0 = new Vector3(0, 0, 0),
     V1 = new Vector3(0, 1, 0),
     V2 = new Vector3(1, 0, 0),
     V3 = new Vector3(1, 1, 0);

     Vector3
       Df0 = Vector3.zero,
       Df1 = Vector3.zero,
       Df2 = Vector3.zero,
       Df3 = Vector3.zero;

    //--------------MultiQuadDisplay-------------//
    // [0] center [1] Top [2] TopRight [3] Right [4] LowerRight [5] Lower [6] LowerLeft [7] Left [8] TopLeft

    [SerializeField]
    Transform[] VertexPositionArray;
    [SerializeField]
    Text[] DeformationVectorTextArray;
    [SerializeField]
    Text[] VertexDataArray;

    float[] MultiQuadForceVectorAngleArray = new float[9];
    Vector3[] MultiQuadVertexArray = 
    {
        new Vector3(0, 0,0),    //4  0
        new Vector3(0, 1,0),    //5  1
        new Vector3(1, 1,0),    //8  2
        new Vector3(1, 0,0),    //7  3
        new Vector3(1, -1,0),   //6  4
        new Vector3(0, -1,0),   //3  5
        new Vector3(-1,-1,0),   //0  6
        new Vector3(-1, 0,0),   //1  7
        new Vector3(-1, 1,0)    //2  8
    };
    Vector3[] MultiQuadDeformationArray = new Vector3[9];
    Vector3[] MultiQuadForceArray = new Vector3[9];

    float[] MultiQuadForceDirectionMod = new float[9];
    float vertexSizeMod = 0.65f;

    private void SetMultiQuadMeshTriangles()
    {
        triangles = new int[]
        {
            6, 7, 5,
            5, 7, 0,
            5, 0, 4,
            4, 0, 3,
            0, 7, 8,
            0, 8, 1,
            3, 0, 1,
            3, 1, 2
        };
    }
    private void UpdateMultiQuadDisplay() 
    {
        // calculate force vector [1] [3] [5] [7]
        float a = 0;
        Vector3 P0 = MultiQuadVertexArray[0] + MultiQuadDeformationArray[0];
        Vector3 Pn = Vector3.zero;
        Vector3 PnP0  = Vector3.zero;
        Vector3 FDf0 = Vector3.zero;
        Vector3 ForceVector = new Vector3(Mathf.Cos(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude, Mathf.Sin(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude, 0);
        VertexPositionArray[0].rotation = Quaternion.EulerAngles(0, 0, ForceAngle * Mathf.Deg2Rad);
        VertexPositionArray[0].localScale = new Vector3(ForceMagnitude * BasalDistance, 16, 1);

        for (int i = 1; i  < 9; i ++)
        {
            Pn = (MultiQuadVertexArray[i] * BasalDistance) + MultiQuadDeformationArray[i];
       
            PnP0 = Pn - P0;

            FDf0 = ForceVector + MultiQuadDeformationArray[0];



            a = Vector3.AngleBetween(PnP0, FDf0) ;
       
                MultiQuadForceDirectionMod[i] =  -Mathf.Cos(a)*ForceMagnitude/BasalDistance;
            
          
              
            
            MultiQuadForceArray[i] = PnP0.normalized * BasalDistance * Mathf.Cos(a);
        
            if(Pn.y >= 0)
            {
                VertexPositionArray[i].rotation = Quaternion.EulerAngles(0, 0, Vector3.AngleBetween(new Vector3(16, 0, 0), Pn));
            }
            else
            {
                VertexPositionArray[i].rotation = Quaternion.EulerAngles(0, 0, -1 *Vector3.AngleBetween(new Vector3(16, 0, 0), Pn));
            }
            if(Vector3.AngleBetween(PnP0, FDf0) > 90*Mathf.Deg2Rad)
            {
                MultiQuadForceArray[i]*=-1;
                VertexPositionArray[i].localScale = new Vector3(MultiQuadForceArray[i].magnitude * -ForceMagnitude, 16, 1);
            }
            else
            {
                VertexPositionArray[i].localScale = new Vector3(MultiQuadForceArray[i].magnitude * ForceMagnitude, 16, 1);
            }

            //VertexPositionArray[i].rotation = Quaternion.EulerAngles(0, 0,(360 * Mathf.Deg2Rad) - (Vector3.AngleBetween( new Vector3(16, 0, 0), MultiQuadForceArray[i]  ) ) );
            
        }
    }
    private void UpdateMultiQuadData()
    {
     
        for (int i = 1; i < 9; i++)
        {
            //Debug.Log("i = " + i);
            VertexDataArray[i].text =
           "Vertex(1, 1) F(" + Mathf.Round(MultiQuadForceArray[i].x * 10f) * 0.1f + "," 
           + Mathf.Round(MultiQuadForceArray[i].y * 10f) * 0.1f + ")"
           + " Fº " + Mathf.Round(MultiQuadForceVectorAngleArray[i] * 10f) * 0.1f + "º";

            DeformationVectorTextArray[i].text =
                "Dfm\n(" + Mathf.Round(MultiQuadDeformationArray[i].x * 10f) * 0.1f + "|" 
                + Mathf.Round(MultiQuadDeformationArray[i].y * 10f) * 0.1f + ")";
        }
    }
    private void UpdateMultiQuadMesh()
    {
        VectorForceDisplay.text = ForceMagnitude.ToString();
        VectorAngleDisplay.text = ForceAngle.ToString();
        UpdateMultiQuadData();

        mesh.Clear();
        vertex = new Vector3[]
        {
            MultiQuadVertexArray[0]*vertexSizeMod + MultiQuadDeformationArray[0]*vertexSizeMod,
            MultiQuadVertexArray[1]*vertexSizeMod + MultiQuadDeformationArray[1]*vertexSizeMod,
            MultiQuadVertexArray[2]*vertexSizeMod + MultiQuadDeformationArray[2]*vertexSizeMod,
            MultiQuadVertexArray[3]*vertexSizeMod + MultiQuadDeformationArray[3]*vertexSizeMod,
            MultiQuadVertexArray[4]*vertexSizeMod + MultiQuadDeformationArray[4]*vertexSizeMod,
            MultiQuadVertexArray[5]*vertexSizeMod + MultiQuadDeformationArray[5]*vertexSizeMod,
            MultiQuadVertexArray[6]*vertexSizeMod + MultiQuadDeformationArray[6]*vertexSizeMod,
            MultiQuadVertexArray[7]*vertexSizeMod + MultiQuadDeformationArray[7]*vertexSizeMod,
            MultiQuadVertexArray[8]*vertexSizeMod + MultiQuadDeformationArray[8]*vertexSizeMod,
        };
        mesh.vertices = vertex;
        mesh.colors = new Color[]
        {
           Color.Lerp(a,b,ForceMagnitude/16),
           Color.Lerp(a,b,MultiQuadForceArray[1].magnitude/16),
           Color.Lerp(a,b,MultiQuadForceArray[2].magnitude/16),
           Color.Lerp(a,b,MultiQuadForceArray[3].magnitude/16),
           Color.Lerp(a,b,MultiQuadForceArray[4].magnitude / 16),
           Color.Lerp(a,b,MultiQuadForceArray[5].magnitude / 16),
           Color.Lerp(a,b,MultiQuadForceArray[6].magnitude / 16),
           Color.Lerp(a,b,MultiQuadForceArray[7].magnitude / 16),
           Color.Lerp(a,b,MultiQuadForceArray[8].magnitude /16),

        };
      
        mesh.triangles = triangles;
    }

    //-------------------------------------------//

    Quaternion Q;

    Vector2 V00F; float V00A; // F = force A = angle
    Vector2 V10F; float V10A;
    Vector2 V01F; float V01A;
    Vector2 V11F; float V11A;
    public void UpdateVertexDataDisplay()
    {
        ForceAngle = AngleSlider.value;
        ForceMagnitude = ForceSlider.value;
        if (viewMode == ViewMode.SingleQuad)
        {
            Vector2 Vr;
            Vector2 Va;
          

            float angle = 0;
            Vertex00.localScale = new Vector3(ForceMagnitude * 16, 16, 1);
            Q = Quaternion.identity;
            Q.SetEulerAngles(0, 0, ForceAngle * Mathf.Deg2Rad);
            Vertex00.SetPositionAndRotation(Vertex00.position, Q);

            V00F = new Vector2(Mathf.Cos(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * BasalDistance, Mathf.Sin(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * BasalDistance);
            Va = new Vector2(V0.x * BasalDistance + Df0.x, V0.y * BasalDistance + Df0.y) + V00F;


            V10F = Vector3.one;
            switch (FPM)
            {
                case ForcePropagationModels.Model1:

                    Vr = Va - new Vector2((V1.x * BasalDistance) - Df1.x, (V1.y * BasalDistance) - Df1.y);

                    if (Vr.magnitude - BasalDistance < 0)
                    {
                        V10F = Vr * -1;
                    }
                    else
                    {
                        V10F = Vr;
                    }
                    //  V10F = new Vector2(Mathf.Cos(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * BasalDistance, Mathf.Sin(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * BasalDistance);
                    break;
                case ForcePropagationModels.Model2:

                    break;
                case ForcePropagationModels.Model3:
                    break;
            }
            Vertex10.localScale = new Vector3(V10F.magnitude, BasalDistance, 1);
            Q = Quaternion.identity;
            angle = Vector3.Angle(new Vector3(16, 0, 0), V10F);
            if (V10F.y < 0) angle = 360 - angle;
            V10A = angle;
            Q.SetEulerAngles(0, 0, angle * Mathf.Deg2Rad);
            Vertex10.SetPositionAndRotation(Vertex10.position, Q);




            V01F = Vector3.one;
            switch (FPM)
            {
                case ForcePropagationModels.Model1:
                    Vr = Va - (new Vector2((V2.x * BasalDistance) - Df2.x, (V2.y * BasalDistance) - Df2.y));

                    if (Vr.magnitude - BasalDistance < 0)
                    {
                        V01F = Vr * -1;
                    }
                    else
                    {
                        V01F = Vr;
                    }



                    //  V01F = new Vector2(Mathf.Cos(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * 16, Mathf.Sin(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * 16);
                    break;
                case ForcePropagationModels.Model2:
                    break;
                case ForcePropagationModels.Model3:
                    break;
            }
            Vertex01.localScale = new Vector3(V01F.magnitude, 16, 1);
            Q = Quaternion.identity;
            angle = Vector2.Angle(new Vector2(16, 0), V01F);
            if (V01F.y < 0) angle = 360 - angle;
            V01A = angle;
            Q.SetEulerAngles(0, 0, angle * Mathf.Deg2Rad);
            Vertex01.SetPositionAndRotation(Vertex01.position, Q);


            V11F = Vector3.one;
            switch (FPM)
            {
                case ForcePropagationModels.Model1:

                    Vr = Va - (new Vector2((V3.x * BasalDistance) - Df3.x, (V3.y * BasalDistance) - Df3.y));

                    if (Vr.magnitude - BasalDistance < 0)
                    {
                        V11F = Vr * -1;
                    }
                    else
                    {
                        V11F = Vr;
                    }

                    // V11F = new Vector2(Mathf.Cos(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * 16, Mathf.Sin(ForceAngle * Mathf.Deg2Rad) * ForceMagnitude * 16);
                    break;
                case ForcePropagationModels.Model2:
                    break;
                case ForcePropagationModels.Model3:
                    break;
            }
            Vertex11.localScale = new Vector3(V11F.magnitude, 16, 1);
            Q = Quaternion.identity;
            angle = Vector2.Angle(new Vector2(16, 0), V11F);
            if (V11F.y < 0) angle = 360 - angle;
            V11A = angle;
            Q.SetEulerAngles(0, 0, angle * Mathf.Deg2Rad);
            Vertex11.SetPositionAndRotation(Vertex11.position, Q);


            UpdateTextData();
        }
        else
        {
            UpdateMultiQuadDisplay();
            UpdateMultiQuadMesh();
        }
        VectorForceDisplay.text = ForceMagnitude.ToString();
        VectorAngleDisplay.text = ForceAngle.ToString();



    }
     
    private void UpdateTextData()
    {
        Vertex00Data.text =
            "Vertex(0, 0) F(" + Mathf.Round(V00F.x * 10f) * 0.1f + "," + Mathf.Round(V00F.y * 10f) * 0.1f + ")" +
            "Fº " + Mathf.Round(ForceAngle * 10f) * 0.1f + "º";
        Vertex01Data.text =
            "Vertex(0, 1) F(" + Mathf.Round(V01F.x * 10f) * 0.1f + "," + Mathf.Round(V01F.y * 10f) * 0.1f + ")" +
            "Fº " + Mathf.Round(V01A * 10f) * 0.1f + "º";
        Vertex10Data.text =
            "Vertex(1, 0) F(" + Mathf.Round(V10F.x * 10f) * 0.1f + "," + Mathf.Round(V10F.y * 10f) * 0.1f + ")" +
            " Fº " + Mathf.Round(V10A * 10f) * 0.1f + "º";
        Vertex11Data.text =
            "Vertex(1, 1) F(" + Mathf.Round(V11F.x * 10f) * 0.1f + "," + Mathf.Round(V11F.y * 10f) * 0.1f + ")" +
            " Fº " + Mathf.Round(V11A * 10f) * 0.1f + "º";


        Df00Data.text = "Dfm\n(" + Mathf.Round(Df0.x * 10f) * 0.1f + "|" + Mathf.Round(Df0.x * 10f) * 0.1f + ")";
        Df01Data.text = "Dfm\n(" + Mathf.Round(Df1.x * 10f) * 0.1f + "|" + Mathf.Round(Df1.x * 10f) * 0.1f + ")";
        Df10Data.text = "Dfm\n(" + Mathf.Round(Df2.x * 10f) * 0.1f + "|" + Mathf.Round(Df2.x * 10f) * 0.1f + ")";
        Df11Data.text = "Dfm\n(" + Mathf.Round(Df3.x * 10f) * 0.1f + "|" + Mathf.Round(Df3.x * 10f) * 0.1f + ")";


    }

    //----------DeformationMesh----------//
    Color a = new Color();
  
        Color b = new Color();

        Color c = new Color();
 
        Color d = new Color();
  
    Vector3[] vertex;
    int[] triangles;
    Mesh mesh;
    public void ComitForceValueChanges()
    {
        float Rdx = 0;
        if (viewMode == ViewMode.SingleQuad)
        {
            Df0 = Df0 + new Vector3(V00F.x, V00F.y, 0) / BasalDistance;
            Df1 = Df1 + new Vector3(V10F.x, V10F.y, 0) / BasalDistance;
            Df2 = Df2 + new Vector3(V01F.x, V01F.y, 0) / BasalDistance;
            Df3 = Df3 + new Vector3(V11F.x, V11F.y, 0) / BasalDistance;
            UpdateDeformationMesh();
        }
        else
        {
            Debug.Log("ComitForceChanges");
            for (int i = 1; i < 9; i++)
            {
                 Rdx = (9 * BasalDistance * BasalDistance) /
                     Mathf.Pow(Vector3.Distance(MultiQuadDeformationArray[0], MultiQuadDeformationArray[i] + MultiQuadVertexArray[i] * BasalDistance)
                    , 2);

                Debug.Log("i = " + i);

                MultiQuadDeformationArray[i] = MultiQuadDeformationArray[i] +
                (MultiQuadForceDirectionMod[i] * (MultiQuadForceArray[i]/Rdx))/BasalDistance;
                
                Debug.Log(Rdx);
                Debug.Log(MultiQuadDeformationArray[i]);
                Debug.Log(MultiQuadForceArray[i]);

                MultiQuadDeformationArray[i].x = Mathf.Clamp(MultiQuadDeformationArray[i].x, -1 * vertexSizeMod, 1 * vertexSizeMod);
                MultiQuadDeformationArray[i].y = Mathf.Clamp(MultiQuadDeformationArray[i].y, -1 * vertexSizeMod, 1 * vertexSizeMod);
            }
            UpdateMultiQuadMesh();
        }
    }
    private void SetDeformationMesh()
    {
        if (viewMode == ViewMode.SingleQuad)
        {
        }
        else
        {

        }
        //vertex = new Vector3[]
        //{
        //    V0 ,
        //    V1 ,
        //    V2 ,
        //    V3 
        //};    
        triangles = new int[]
        {
            0, 1, 2,
            1, 3, 2,
        };
    }
    private void UpdateDeformationMesh()
    {

        VectorForceDisplay.text = ForceMagnitude.ToString();
        VectorAngleDisplay.text = ForceAngle.ToString();
        UpdateTextData();

        mesh.Clear();
        vertex = new Vector3[]
       {
            V0 + Df0/BasalDistance,
            V1 + Df1/BasalDistance,
            V2 + Df2/BasalDistance ,
            V3 + Df3/BasalDistance 
       };
        mesh.vertices = vertex;
        mesh.colors = new Color[]
        {
           a, 
           b, 
           c, 
           d
        };
        mesh .triangles = triangles;
    }//SingleQuad
    public void ResetMesh()
    {
        if(viewMode == ViewMode.SingleQuad)
        {
            Df0 = Vector3.zero;
            Df1 = Vector3.zero;
            Df2 = Vector3.zero;
            Df3 = Vector3.zero;

            UpdateDeformationMesh();
            UpdateTextData();
        }
        else
        {
            for (int i = 0; i < 9; i++)
            {
                MultiQuadDeformationArray[i] = new Vector2(0, 0);
                UpdateMultiQuadData();
                UpdateMultiQuadDisplay();
                UpdateMultiQuadMesh();
            }
        }
    

    }
    public void SwitchViewMode()
    {
        
        if (viewMode == ViewMode.MultiQuad)
        {
            viewMode = ViewMode.SingleQuad;
            SingelQuad.SetActive(false);
            MultiQuad.SetActive(true);
        }
        else
        {
            viewMode = ViewMode.MultiQuad;
            SingelQuad.SetActive(true);
            MultiQuad.SetActive(false);
        }
    }
}
