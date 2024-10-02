using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class obfImporter : MonoBehaviour
{

    [SerializeField]
    string path = "Assets/buddha.off";

    int nbSommets;
    int nbFacettes;
    int nbAretes;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        List<Vector3> verticesList = new();
        List<int> triangles = new();

        const Int32 BufferSize = 128;
        using (var fileStream = File.OpenRead(path))
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
            String line = streamReader.ReadLine();

            // Parsing first line to check file format
            if (line == null || line != "OFF")
            {
                Console.Error.WriteLine("Not the correct file format : Aborting");
                return;
            }

            if ((line = streamReader.ReadLine()) != null)
            {
                string[] values = line.Split(' ');
                nbSommets = int.Parse(values[0]);
                nbFacettes = int.Parse(values[1]);
                nbAretes = int.Parse(values[2]);
            }

            for (int i = 0; i < nbSommets; i++)
            {
                line = streamReader.ReadLine();
                if (line == null)
                {
                    return;
                }
                string[] values = line.Replace(".", ",").Split(' ');
                verticesList.Add(new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2])));
            }

            for (int i = 0; i < nbFacettes; i++)
            {
                line = streamReader.ReadLine();
                if (line == null)
                {
                    return;
                }
                string[] values = line.Split(' ');
                triangles.Add(int.Parse(values[1]));
                triangles.Add(int.Parse(values[2]));
                triangles.Add(int.Parse(values[3]));
            }

            while ((line = streamReader.ReadLine()) != null)
            {
                Debug.Log(line);
            }
        }


        Vector3 gravCenter = new(0, 0, 0);
        Vector3 biggest = new(0, 0, 0);
        var arrVertices = verticesList.ToArray();

        //Calcul du centre de gravité afin de centrer le modèle et recherche de la plus grande coordonnée (en absolue)
        foreach (Vector3 v in arrVertices)
        {
            gravCenter += v;
        }
        gravCenter /= nbSommets;

        //On recentre tout
        for (int i = 0; i < arrVertices.Length; i++)
        {
            arrVertices[i] -= gravCenter;
        }

        //On calcule ensuite le plus éloigné grâce à la magnitude (vu que l'on est centré en 0,0,0)
        foreach (Vector3 v in arrVertices)
        {
            if (v.magnitude > biggest.magnitude)
            {
                biggest = v;
            }
        }

        //On normalise la taille de l'objet grâce a biggest
        for (int i = 0; i < arrVertices.Length; i++)
        {
            arrVertices[i] /= biggest.magnitude;
        }

        // --- Ici on calcule les normales de chaque vertex ---

        // On réserve de l'espace pour les normales (initialisé a 0,0,0)
        Vector3[] vertexNormal = new Vector3[verticesList.Count];
        int[] triCountVert = new int[verticesList.Count];

        for (int i = 0; i < triangles.Count; i += 3)
        {
            //Array pour pouvoir itérer dessus plus tard
            int[] triangleVertex = new int[3];

            triangleVertex[0] = triangles[i];
            triangleVertex[1] = triangles[i + 1];
            triangleVertex[2] = triangles[i + 2];

            //On calcule les deux côtés du triangle
            Vector3 a = arrVertices[triangleVertex[0]] - arrVertices[triangleVertex[1]];
            Vector3 b = arrVertices[triangleVertex[0]] - arrVertices[triangleVertex[2]];

            //On calcule la normale à la surface
            Vector3 surfaceNormal = Vector3.Cross(a, b).normalized;

            //On additionne toutes les normales de suface correspondant aux vertex
            for (int j = 0; j < 3; j++)
            {
                vertexNormal[triangleVertex[j]] += surfaceNormal; 
                triCountVert[triangleVertex[j]] += 1;
            }
        }

        // On fait la moyenne
        for (int i = 0; i < triCountVert.Length; i++)
        {
            vertexNormal[i] /= triCountVert[i];
        }

        // On met le modèle dans le mesh
        mesh.vertices = arrVertices;
        mesh.triangles = triangles.ToArray();
        mesh.normals = vertexNormal;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
