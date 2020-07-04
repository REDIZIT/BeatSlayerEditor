using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlaySongScript : MonoBehaviour
{
    public AudioSource asrc;

    public GameObject cam_editor, cam_move, cam_play;
    public GameObject onlyEditor, onlyPlay;
    SongEditor se
    {
        get
        {
            return cam_editor.GetComponent<SongEditor>();
        }
    }

    public GameObject BeatCubePrefab, BeatLinePrefab;
    public SpawnPointScript[] spawnPoints;
    List<SpawnPointClass> spawnPointClasses = new List<SpawnPointClass>();
    public List<BeatCubeClass> beats;
    public bool isPlaying;
    public bool paused;
    public float pitch;
    public Button backBtn;
    public float startTime; // Время трека когда нажали Play
    public void Play(Project project)
    {
        if (isPlaying) return;

        startTime = asrc.time;

        isPlaying = true;
        cam_editor.SetActive(false);
        onlyEditor.SetActive(false);
        cam_move.SetActive(false);

        cam_play.SetActive(true);
        onlyPlay.SetActive(true);

        //asrc.time = se.aSource.time;
        se.aSource.Pause();

        backBtn.interactable = false;

        beats.Clear();
        beats.AddRange(project.beatCubeList.Where(c => c.time >= asrc.time));
        List<BeatCubeClass> sorted = new List<BeatCubeClass>();
        beats = beats.OrderBy(c => c.time).ToList();

        pitch = asrc.pitch;
        StartCoroutine(IPlay());
    }
    public IEnumerator IPlay()
    {
        gameStarting = true;
        gameStarted = true;
        yield return new WaitForSeconds(0.7f / pitch / 1);
        backBtn.interactable = true;
        asrc.Play();
        if (paused) { asrc.Pause(); }
        gameStarting = false;
    }

    public void Stop()
    {
        cam_editor.SetActive(true);
        onlyEditor.SetActive(true);
        cam_move.SetActive(true);

        cam_play.SetActive(false);
        onlyPlay.SetActive(false);

        gameStarting = false;
        gameStarted = false;

        asrc.time = startTime;
        asrc.Pause();

        isPlaying = false;
    }





    private void Update()
    {
        if (!isPlaying) return;

        if (!paused && gameStarted)
        {
            if (gameStarting)
            {
                asReplacer += Time.deltaTime;
            }
            if (paused)
            {
                if (asrc.isPlaying)
                {
                    asrc.Pause();
                }
                return;
            }

            ProcessBeatTime();
        }


        if (!gameStarting && !paused && !asrc.isPlaying && beats.ToArray().Length == 0)
        {
            if (!gameCompleted)
            {
                gameCompleted = true;

                Debug.Log("The End");
            }
        }

        #region Slicing

        if (Application.isEditor)
        {
            Ray ray = cam_play.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.name == "LineCollider")
                {
                    hit.transform.parent.GetComponent<Bit>().Spec(hit.point);
                }
                else if (hit.transform.GetComponent<Bit>() != null && !hit.transform.GetComponent<Bit>().isDead)
                {
                    Bit bit = hit.transform.GetComponent<Bit>();
                    bit.Sliced();
                }
            }
        }
        else
        {
            for (int i = 0; i < Input.touches.Length; i++)
            {
                Ray ray = cam_play.GetComponent<Camera>().ScreenPointToRay(Input.touches[i].position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.name == "LineCollider")
                    {
                        hit.transform.parent.GetComponent<Bit>().Spec(hit.point);
                    }
                    else if (hit.transform.GetComponent<Bit>() != null && !hit.transform.GetComponent<Bit>().isDead)
                    {
                        hit.transform.GetComponent<Bit>().Sliced(i);
                    }
                }
            }
        }

        foreach (SpawnPointClass c in spawnPointClasses)
        {
            if (c.cooldown > 0)
            {
                c.cooldown -= Time.deltaTime;
            }
        }

        #endregion
    }

    bool gameCompleted, gameStarted, gameStarting;
    float asReplacer = 0;

    public void ProcessBeatTime()
    {
        float asTime = gameStarting ? asReplacer : asrc.time + 0.7f / pitch;
        if (beats.ToArray().Length > 0 && asTime >= beats[0].time)
        {
            SpawnBeatCube(beats[0], Color.white);

            beats.RemoveAt(0);
            ProcessBeatTime();
        }
    }
    public void SpawnBeatCube(BeatCubeClass beat, Color color)
    {
        bool noArrows = false;
        bool noLines = false;
        bool displayColor = false;

        BeatCubeClass.Type type = beat.type;
        if (type == BeatCubeClass.Type.Dir && noArrows)
        {
            type = BeatCubeClass.Type.Point;
        }
        else if (type == BeatCubeClass.Type.Line && noLines)
        {
            return;
        }

        GameObject c = Instantiate(beat.type == BeatCubeClass.Type.Line ? BeatLinePrefab : BeatCubePrefab);

        //GameObject c = lean.Show(beat.type);
        //c.GetComponent<Bit>().Start();


        if (displayColor)
        {
            c.GetComponent<MeshRenderer>().materials[1].SetColor("_Color", color);
        }

        //c.GetComponent<Bit>().speed *= cubesspeed;
        c.GetComponent<Bit>().useSoundEffect = true;

        c.GetComponent<Bit>().type = type;
        c.GetComponent<Bit>().subType = beat.subType;

        if (beat.type == BeatCubeClass.Type.Line)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < beat.linePoints.Count; i++)
            {
                Vector3 v3 = beat.linePoints[i];
                points.Add(v3);
            }
            c.GetComponent<Bit>().SpecSpawn(points.ToArray());
        }

        //int road = bcc[0].road == -1 ? GetBestSpawnPoint(c.GetComponent<Bit>().sliceDir) : bcc[0].road;
        //int road = bcc[0].road;
        int road = beat.road == -1 ? GetBestSpawnPoint(beat) : beat.road;

        if (road >= spawnPoints.Length) road = Random.Range(0, 3);
        Transform selectedSpawnPoint = spawnPoints[road].transform;
        c.transform.position = new Vector3(selectedSpawnPoint.position.x, 1, selectedSpawnPoint.position.z);
        spawnPoints[road].Spawn(beat.type);
        c.transform.name = "BeatCube";
        //cubesSpawned++;

        c.GetComponent<Bit>().Start();
    }

    public int GetBestSpawnPoint(BeatCubeClass beat)
    {
        List<SpawnPointClass> spawnPoints = spawnPointClasses.OrderBy(o => o.cooldown).ToList();

        if (beat.type == BeatCubeClass.Type.Dir)
        {
            List<int> available = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (spawnPoints[i].cooldown <= 0)
                {
                    available.Add(i);
                }
            }
            int rnd = Random.Range(0, available.ToArray().Length);
            spawnPoints[rnd].cooldown = 0.3f;
            return spawnPoints[rnd].index;
        }
        else
        {
            int rnd = Random.Range(0, spawnPoints.ToArray().Length);
            spawnPoints[rnd].cooldown = 0.3f;
            return spawnPointClasses[rnd].index;
        }
    }

}

class SpawnPointClass
{
    public int index;
    public float cooldown;
}