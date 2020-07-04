using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bit : MonoBehaviour
{

    //
    // Скрипт Bit находится в папке с редактором, так что
    // тут можно ломать всё что захочеться ;D
    //
    // P.S. Хотя может быть всё не стоит ломать
    //

    public float speed;
    public float maxDistance;
    public bool useSoundEffect;
    [HideInInspector] public int touch;
    [HideInInspector] public Vector3 startPos, endPos, dir;
    [HideInInspector] public float slicePow;
    public Mesh dirMesh;
    public Mesh pointMesh;

    public BeatCubeClass.Type type;
    public BeatCubeClass.SubType subType;

    public BeatCubeClass.SubType bitSubType;

    public GameObject cubeSlicePs;

    public ParticleSystem linePsLoop, linePsDestroy;

    [HideInInspector] public float prevDistPerFrame = 0;

    public void Start()
    {
        if(type == BeatCubeClass.Type.Line)
        {
            StartForLine();
        }
        else
        {
            StartForCube();
        }
    }
    void StartForCube()
    {
        cubeSlicePs.GetComponent<ParticleSystem>().Stop();
        cubeSlicePs.transform.position = transform.position;

        GetComponent<BoxCollider>().enabled = true;

        GetComponent<MeshRenderer>().materials[0].SetFloat("_Threshold", -0.1f);
        GetComponent<MeshRenderer>().materials[1].SetFloat("_Threshold", -0.1f);
        GetComponent<MeshRenderer>().materials[2].SetFloat("_Threshold", -0.1f);


        if (type == BeatCubeClass.Type.Point)
        {
            GetComponent<MeshFilter>().mesh = pointMesh;
        }
        else
        {
            GetComponent<MeshFilter>().mesh = dirMesh;
        }

        float r = Random.Range(-100f, 100f);
        GetComponent<MeshRenderer>().materials[0].SetFloat("_Offset", r);
        GetComponent<MeshRenderer>().materials[1].SetFloat("_Offset", r);
        GetComponent<MeshRenderer>().materials[2].SetFloat("_Offset", r);



        if (subType == BeatCubeClass.SubType.Random)
        {
            int rnd = Random.Range(0, 4);
            if (rnd == 0) { bitSubType = BeatCubeClass.SubType.Down; }
            else if (rnd == 1) { bitSubType = BeatCubeClass.SubType.Up; }
            else if (rnd == 2) { bitSubType = BeatCubeClass.SubType.Left; }
            else { bitSubType = BeatCubeClass.SubType.Right; }
        }
        else
        {
            bitSubType = subType;
        }
        int zRot = bitSubType == BeatCubeClass.SubType.Up ? 180 :
           bitSubType == BeatCubeClass.SubType.Down ? 0 :
           bitSubType == BeatCubeClass.SubType.Left ? 270 :
           90;
        transform.eulerAngles = new Vector3(0, 0, zRot);
    }
    void StartForLine()
    {
        line = GetComponent<LineRenderer>();
        
        sphere.SetActive(false);
        sphereCap.SetActive(true);
        sphereEndCap.SetActive(false);

        ResetLine();
    }

    void ResetLine()
    {
        if (linePsLoop == null)
        {
            sphere = transform.GetChild(0).gameObject;
            sphereCap = transform.GetChild(1).gameObject;
            sphereEndCap = transform.GetChild(2).gameObject;
            linePsLoop = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
            linePsDestroy = transform.GetChild(0).GetChild(2).GetComponent<ParticleSystem>();
        }
        linePsLoop.transform.parent = sphere.transform;
        linePsLoop.transform.localPosition = Vector3.zero;
        linePsDestroy.transform.parent = sphere.transform;
        linePsDestroy.transform.localPosition = Vector3.zero;
    }

    public GameObject sphere, sphereCap, sphereEndCap;
    public List<Vector3> linePoints = new List<Vector3>();
    public List<Vector3> spawnLinePoints = new List<Vector3>();

    public void SpecSpawn(Vector3[] points)
    {
        linePoints = new List<Vector3>();
        spawnLinePoints = new List<Vector3>();
        foreach (Vector3 point in points)
        {
            spawnLinePoints.Add(new Vector3(point.x, point.y, point.z * 60));
        }

        linePoints.Add(spawnLinePoints[0]);
        linePoints.Add(spawnLinePoints[0]);

        ResetLine();
    }
    public float spawnEffect = 0;
    public float distToNext;

    public int spawningId = 1;
    public void SpecSpawnUpdater()
    {
        if (spawningId == -1) return;

        // Получаем последний элемент в списке
        if (linePoints.Count < 2) return;
        int i = linePoints.ToArray().Length - 1;

        // Получаем нормаль движения
        Vector3 dir = (spawnLinePoints[spawningId] - spawnLinePoints[spawningId - 1]).normalized;
        // Рачтитываем точку соприкосновения LineRenderer и спавн поинта
        spawnEffect = 43 - transform.position.z + dir.sqrMagnitude;

        // Находим расстояние между запларинуемыми точками
        distToNext = spawnLinePoints[spawningId].z - spawnLinePoints[spawningId - 1].z;
        // Находим расстояние от отрисованной части до спавн поинта
        float dist = spawnEffect - linePoints[i - 1].z;
        // Увеличиваем отрисованую часть
        Vector3 prevKey = linePoints[i - 1];
        prevKey += new Vector3(dist * dir.x, dist * dir.y, dist * dir.z);
        linePoints[i] = prevKey;

        foreach(Transform child in transform)
        {
            if(child.name == "LineCollider")
            {
                Destroy(child.gameObject);
                //child.gameObject.SetActive(false);
            }
        }
        for (int s = 0; s < linePoints.Count - 1; s++)
        {
            AddColliderToLine(line, linePoints[s], linePoints[s + 1]);
        }

        // Если расстояние от отрисовнной части до запланированной < 0
        if (spawnLinePoints[spawningId].z - linePoints[i].z < 0)
        {
            // Выравниваем
            linePoints[i] = spawnLinePoints[spawningId];

            // Если конец
            if(spawningId >= spawnLinePoints.ToArray().Length - 1)
            {
                spawningId = -1;
                sphereEndCap.transform.localPosition = spawnLinePoints[spawnLinePoints.Count - 1];
                //Debug.Log("123");
                sphereEndCap.SetActive(true);
            }
            else
            {
                spawningId++;
                linePoints.Add(spawnLinePoints[spawningId - 1]);
            }
        }
    }
    public void Spec(Vector3 hit)
    {
        if (isDead) return;

        float localHit = hit.z - transform.position.z;
        if (!sphere.activeSelf) { sphere.SetActive(true); linePsLoop.Play(); sphereCap.SetActive(false); }
        //sphere.transform.position = new Vector3(hit.x, transform.position.y, hit.z);
        float distToNext = linePoints[1].z - linePoints[0].z;
        float dist = localHit - linePoints[0].z;
        Vector3 prevKey = linePoints[0];
        Vector3 dir = (linePoints[1] - linePoints[0]).normalized;
        prevKey += new Vector3(dir.x * dist, dir.y * dist, dist);
        linePoints[0] = prevKey;
        sphere.transform.localPosition = linePoints[0];
        if(dist >= distToNext)
        {
            linePoints.RemoveAt(0);
        }

        if(spawningId == -1)
        {
            if (spawnLinePoints[spawnLinePoints.Count - 1].z - linePoints[0].z <= 1.25f)
            {
                linePsLoop.transform.parent = null;
                linePsLoop.Stop();
                linePsDestroy.Play();
                linePsDestroy.transform.parent = null;

                Destroy(gameObject);
            }
        }
    }
    public void SpecDieAnim()
    {
        Color emission = line.material.color;
        emission = new Color(emission.r / 1.1f, emission.g / 1.2f, emission.b / 1.2f, emission.a / 1.2f);
        //line.material.SetColor("Color", emission);
        line.material.color = new Color(emission.a, emission.a, emission.a, emission.a);

        if(emission.a <= 0.1f)
        {
            linePsLoop.transform.parent = null;
            linePsLoop.Stop();

            linePsDestroy.transform.parent = null;

            foreach (Transform child in transform)
            {
                if (child.name == "LineCollider")
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            Destroy(gameObject);
        }
    }

    LineRenderer line;
    private void AddColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        //create the collider for the line
        BoxCollider lineCollider = new GameObject("LineCollider").AddComponent<BoxCollider>();
        //set the collider as a child of your line
        lineCollider.transform.parent = line.transform;
        // get width of collider from line 
        float lineWidth = line.endWidth * 2.5f;
        // get the length of the line using the Distance method
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        lineCollider.size = new Vector3(lineLength, lineWidth, lineWidth);
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.localPosition = midPoint;


        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));

        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;

        //were interested in the inverse so multiply by -1
        angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.Rotate(0, angle, 0);
    }



    public float deadThresgold = 0;
    public bool Update_IsDead()
    {
        if (isDead)
        {
            try
            {
                GetComponent<MeshRenderer>().materials[0].SetFloat("_Threshold", deadThresgold);
                GetComponent<MeshRenderer>().materials[1].SetFloat("_Threshold", deadThresgold);
                GetComponent<MeshRenderer>().materials[2].SetFloat("_Threshold", deadThresgold);
                deadThresgold += Time.deltaTime * 2f;
                speed -= (speed - Time.deltaTime) / 8f;
                if (deadThresgold >= 1 /*&& !cubeSlicePs.GetComponent<ParticleSystem>().isPlaying && gameObject.activeSelf*/)
                {
                    Destroy(gameObject);
                }

                transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;
            }
            catch (System.Exception err)
            {
                Debug.LogError("IsDead error: Materials count: " + GetComponent<MeshRenderer>().materials.Length + " Name: " + transform.name + " msg:" + err.Message);
            }

            return true;
        }
        return false;
    }

    public string debugString;
    private void Update()
    {
        System.TimeSpan t = System.DateTime.Now.TimeOfDay;;

        if (type == BeatCubeClass.Type.Line)
        {
            UpdateForLine();
        }
        else
        {
            if (Update_IsDead()) return;

            UpdateForCube();
        }

        //Debug.Log("Bit.Update: " + (System.DateTime.Now.TimeOfDay - t).TotalMilliseconds + "ms");
    }
    void UpdateForCube()
    {
        transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;
        if (transform.position.z <= maxDistance)
        {
            System.TimeSpan t = System.DateTime.Now.TimeOfDay;

            Destroy(gameObject);
        }

        if (sliced)
        {
            if (Application.isEditor)
            {
                endPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                slicePow = (endPos.x - startPos.x) + (endPos.y - startPos.y);
                slicePow *= 5f;
            }
            else
            {
                try
                {
                    if (Input.GetTouch(touch).fingerId != currentTouchId)
                    {
                        endPos = startPos;
                        return;
                    }
                }
                catch
                {
                    return;
                }
                endPos = new Vector3(Input.GetTouch(touch).position.x, Input.GetTouch(touch).position.y, 0);
                slicePow = (endPos.x - startPos.x) + (endPos.y - startPos.y);
            }
            dir = (endPos - startPos);

            if (type == BeatCubeClass.Type.Point)
            {
                if (!Application.isEditor)
                {
                    if (Input.GetTouch(touch).phase == TouchPhase.Began)
                    {
                        SendBitSliced();
                    }
                }
                else
                {
                    SendBitSliced();
                }
            }
            else if (bitSubType == BeatCubeClass.SubType.Down) // DOWN
            {
                if (dir.normalized.y <= -0.6f) SendBitSliced();
            }
            else if (bitSubType == BeatCubeClass.SubType.Up) // UP
            {
                if (dir.normalized.y >= 0.6f) SendBitSliced();
            }
            else if (bitSubType == BeatCubeClass.SubType.Left) // LEFT
            {
                if (dir.normalized.x <= -0.6f) SendBitSliced();
            }
            else if (bitSubType == BeatCubeClass.SubType.Right) // RIGHT
            {
                if (dir.normalized.x >= 0.6f) SendBitSliced();
            }
            else
            {
                sliced = false;
            }
        }
    }
    void UpdateForLine()
    {
        transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;

        if (linePoints[0].z + transform.position.z <= maxDistance) isDead = true;
        if (isDead) SpecDieAnim();

        if (isDead) return;

        SpecSpawnUpdater();

        line.positionCount = linePoints.ToArray().Length;
        for (int i = 0; i < linePoints.ToArray().Length; i++)
        {
            line.SetPosition(i, linePoints[i]);
        }
    }


    public bool isDead;
    [HideInInspector] public int currentTouchId;
    public void SendBitSliced()
    {
        try
        {
            if (useSoundEffect)
            {
                cubeSlicePs.SetActive(true);
                cubeSlicePs.transform.parent = null;
            }
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Bit error catched #1: " + err);
        }

        try
        {
            isDead = true;
            GetComponent<BoxCollider>().enabled = false;
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Bit error catched #2: " + err);
        }
        

        try
        {
            foreach (Material mat in GetComponent<MeshRenderer>().materials)
            {
                mat.SetColor("_Color", new Color(
                    mat.color.r / 10f,
                    mat.color.g / 10f,
                    mat.color.b / 10f));
            }
        }
        catch(System.Exception err)
        {
            Debug.LogWarning("Bit error catched #3: " + err);
        }

        Destroy(gameObject);
    }
    public void Sliced(int t)
    {
        if (sliced) return;
        if (transform.position.z <= 14)
        {
            touch = t;
            currentTouchId = Input.touches[t].fingerId;


            if (Input.GetTouch(t).phase != TouchPhase.Began && type == BeatCubeClass.Type.Point)
            {
                return;
            }
            startPos = new Vector3(Input.GetTouch(t).position.x, Input.GetTouch(t).position.y, 0);
            sliced = true;

        }
    }

    public bool sliced = false;
    public void Sliced()
    {
        if (sliced)
        {
            return;
        }
        if (transform.position.z <= 20)
        {
            startPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            sliced = true;
        }
    }
}