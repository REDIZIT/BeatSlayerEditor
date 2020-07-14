using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using static ModerationUI;

namespace ProjectManagement
{
    public static class ProjectManager
    {
        public static ProjectListItem[] GetProjectsLegacy()
        {
            string mapsFolder = Application.persistentDataPath + "/Projects";
            string[] maps = Directory.GetFiles(mapsFolder).Where(c => Path.GetExtension(c) == ".bsz").ToArray();

            ProjectListItem[] projects = new ProjectListItem[maps.Length];
            for (int i = 0; i < maps.Length; i++)
            {
                string trackname = Path.GetFileNameWithoutExtension(maps[i]);
                projects[i] = new ProjectListItem()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    coverPath = TheGreat.GetCoverPath(trackname)
                };
            }

            return projects;
        }
        public static ProjectListItem[] GetProjects()
        {
            string mapsFolder = Application.persistentDataPath + "/Maps";
            string[] maps = Directory.GetDirectories(mapsFolder);

            ProjectListItem[] projects = new ProjectListItem[maps.Length];
            for (int i = 0; i < maps.Length; i++)
            {
                string trackname = new DirectoryInfo(maps[i]).Name;
                projects[i] = new ProjectListItem()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    coverPath = TheGreat.GetCoverPath(trackname)
                };
            }

            return projects;
        }

        /// <summary>
        /// Load project from map folder
        /// </summary>
        /// <param name="item">Main info</param>
        /// <param name="loadAll">Load audio and cover from folder?</param>
        /// <returns></returns>
        public static Project LoadProject(ProjectListItem item, bool loadAll = false)
        {
            string trackname = item.author + "-" + item.name;
            string mapFolder = Application.persistentDataPath + "/Maps/" + trackname;
            string projectPath = mapFolder + "/" + trackname + ".bsu";

            Project proj = LoadProject(projectPath);


            if (!loadAll) return proj;

            string audioPath = mapFolder + "/" + trackname + Project.ToString(proj.audioExtension);
            string coverPath = mapFolder + "/" + trackname + Project.ToString(proj.imageExtension);

            proj.audioFile = File.ReadAllBytes(audioPath);

            if(File.Exists(coverPath)) proj.image = File.ReadAllBytes(coverPath);

            return proj;
        }
        public static Project LoadProject(ModerateOperation op)
        {
            string perstPath = Application.persistentDataPath.Replace("com.REDIZIT.BeatSlayerEditor", "com.REDIZIT.BeatSlayer");
            if (Application.isEditor) perstPath = Application.persistentDataPath.Replace("Beat Slayer Editor", "Beat Slayer");

            string folder = perstPath + "/data/moderation/map";

            if (!Directory.Exists(folder)) return null;

            string bsuPath = folder + "/" + op.trackname + ".bsu";
            if (string.IsNullOrWhiteSpace(bsuPath)) return null;
            if (!File.Exists(bsuPath)) return null;

            return LoadProject(bsuPath);
        }
        /// <summary>
        /// Load only bsu file project
        /// </summary>
        public static Project LoadProject(string bsuPath)
        {
            Project proj;
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (Stream stream = File.OpenRead(bsuPath))
            {
                proj = (Project)xml.Deserialize(stream);
            }

            return proj;
        }


        public static Project UnpackBszFile(string bszFilePath, string mapFolder)
        {
            Project proj;

            XmlSerializer xml = new XmlSerializer(typeof(Project));
            FileStream loadStream = File.OpenRead(bszFilePath);
            proj = (Project)xml.Deserialize(loadStream);
            loadStream.Close();
            
            string targetFilesPath = mapFolder + "/" + (proj.author.Trim() + "-" + proj.name.Trim());

            if (!Directory.Exists(mapFolder)) Directory.CreateDirectory(mapFolder);
            
            // Unpack audio file
            string audioPath = targetFilesPath + (proj.audioExtension == Project.AudioExtension.Mp3 ? ".mp3" : ".ogg");
            File.WriteAllBytes(audioPath, proj.audioFile);
            proj.audioFile = null;

            // Unpack image file
            if (proj.hasImage)
            {
                string imagePath = targetFilesPath + (proj.imageExtension == Project.ImageExtension.Jpeg ? ".jpg" : ".png");
                File.WriteAllBytes(imagePath, proj.image);
                proj.image = null;
            }

            // Saving unpacked file (in .bsu) into target folder
            Stream saveStream = File.Create(targetFilesPath + ".bsu");
            xml.Serialize(saveStream, proj);
            saveStream.Close();

            return proj;
        }
        
        
        
        public static IEnumerator LoadAudioClip(Project project, Action<AudioClip> callback)
        {
            string path = Application.persistentDataPath + "/Maps/" + project.author + "-" + project.name + "/" + project.author + "-" + project.name + Project.ToString(project.audioExtension);
            using (WWW www = new WWW("file:///" + path))
            {
                yield return www;
                callback(www.GetAudioClip());
            }
        }

        public static Sprite LoadCover(Project project)
        {
            string path = TheGreat.GetCoverPath(project.author + "-" + project.name);
            if (path == "") return null;
            else return TheGreat.LoadSprite(path);
        }
        
        
        public static void CreateProject(Project source)
        {
            string folder = Application.persistentDataPath + "/Maps/" + source.author + "-" + source.name;
            string projectPath = folder + "/" + source.author + "-" + source.name + ".bsu";
            string audioPath = folder + "/" + source.author + "-" + source.name + Project.ToString(source.audioExtension);
            string coverPath = source.hasImage ? folder + "/" + source.author + "-" + source.name + Project.ToString(source.imageExtension) : "";

            if(Directory.Exists(folder)) { Debug.LogError("This project folder already exists"); return; }

            Directory.CreateDirectory(folder);

            // Extract music file
            File.WriteAllBytes(audioPath, source.audioFile);
            source.audioFile = null;

            // Extract cover file
            if (source.hasImage)
            {
                File.WriteAllBytes(coverPath, source.image);
            }
            source.image = null;

            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using(var stream = File.Create(projectPath))
            {
                xml.Serialize(stream, source);
            }
        }

        /// <summary>
        /// Save project from Project into folder
        /// </summary>
        /// <param name="proj">Project to save</param>
        /// <param name="saveAll">Extract audio and cover image from Project into folder?</param>
        public static void SaveProject(Project proj, bool saveAll = false)
        {
            string folder = Application.persistentDataPath + "/Maps/" + proj.author + "-" + proj.name;
            string projectPath = folder + "/" + proj.author + "-" + proj.name + ".bsu";

            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (var stream = File.Create(projectPath + ".tmp"))
            {
                xml.Serialize(stream, proj);
            }

            File.Delete(projectPath);
            File.Move(projectPath + ".tmp", projectPath);


            if (!saveAll) return;
            
            string audioPath = folder + "/" + proj.author + "-" + proj.name + Project.ToString(proj.audioExtension);
            string coverPath = folder + "/" + proj.author + "-" + proj.name + Project.ToString(proj.imageExtension);

            File.WriteAllBytes(audioPath, proj.audioFile);

            if(proj.image != null && proj.image.Length > 0)
            {
                File.WriteAllBytes(coverPath, proj.image);
            }
        }

        public static void RenameProject(Project proj, string name)
        {
            string oldTrackname = proj.author + "-" + proj.name;
            string newTrackname = name;

            string mapsFolder = Application.persistentDataPath + "/Maps";
            string oldMapFolder = mapsFolder + "/" + oldTrackname;
            string newMapFolder = mapsFolder + "/" + newTrackname;

            Directory.Move(oldMapFolder, newMapFolder);
            foreach (string file in Directory.GetFiles(newMapFolder))
            {
                string ext = Path.GetExtension(file);
                File.Move(file, newMapFolder + "/" + newTrackname + ext);
            }

            proj.author = newTrackname.Split('-')[0];
            proj.name = newTrackname.Split('-')[1];
        }

        public static void CompressProject(ProjectListItem proj, string destination)
        {
            Project compressed = LoadProject(proj, true);

            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using(Stream stream = File.Create(destination))
            {
                xml.Serialize(stream, compressed);
            }
        }
        public static void CompressProject(Project compressed, string destination)
        {
            XmlSerializer xml = new XmlSerializer(typeof(Project));
            using (Stream stream = File.Create(destination))
            {
                xml.Serialize(stream, compressed);
            }
        }


        public static void DeleteProject(ProjectListItem proj)
        {
            string trackname = proj.author + "-" + proj.name;
            string mapFolder = Application.persistentDataPath + "/Maps/" + trackname;

            Directory.Delete(mapFolder, true);
        }
    }

    public class ProjectListItem
    {
        public string author, name, time;
        public string coverPath;

        public int difficultyStars;
    }


    public class GroupInfo
    {
        public string author, name;
        public int mapsCount;
    }
    public class MapInfo
    {
        public GroupInfo group;

        //public string author { get { return group.author; } }
        //public string name { get { return group.name; } }

        public string nick;

        public int likes, dislikes, playCount, downloads;

        public string difficultyName;
        public int difficultyStars;

        public DateTime publishTime;

        public bool granted;
        public DateTime grantedTime;

        public bool IsGrantedNow
        {
            get
            {
                if (!granted) return false;
                else return grantedTime > publishTime;
            }
        }

        public MapInfo() { }
        public MapInfo(GroupInfo group)
        {
            this.group = group;
        }
    }
    
    public class DifficultyInfo
    {
        public string name;
        public int stars;
        public int id = -1;

        public int downloads, playCount, likes, dislikes;
    }
}
