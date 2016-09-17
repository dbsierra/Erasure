using UnityEngine;
using System.Collections;
using UnityEngine.Windows;
using System.IO;

public class RootController : MonoBehaviour {

    public enum States
    {
        ready=0,
        camera=1,
        averaging=2,
        morphing=3
    };
    States state;

    Texture2D PCurrent;
    Texture2D PAverage;
    WebCamTexture PWebCam;
    Vector4[] PSigmaColors;

    public MeshRenderer Quad;
    Material mat;

    //morphing
    float startTime = 0;
    public float MorphLength;

    int counter;

    public GameObject Guides;

    public AudioSource CameraSound;
    public AudioSource AverageSound;
    AudioClip AAverage;
    AudioClip ACurrent;
    AudioClip ASum;
    Microphone mic;

	// Use this for initialization
	void Start ()
    {
        PWebCam = new WebCamTexture(WebCamTexture.devices[1].name);
        PWebCam.Play();
       
        //Init
        PCurrent = new Texture2D(PWebCam.width, PWebCam.height, TextureFormat.RGB24, false);
        PAverage = new Texture2D(PWebCam.width, PWebCam.height, TextureFormat.RGB24, false);
        PSigmaColors = new Vector4[PWebCam.GetPixels().Length];
        counter = 1;
        state = States.ready;
        mat = Quad.material;
        //mic = new Microphone();

        
        //Microphone.Start( Microphone.devices[0], true,  );

        EnterCameraState();
        state = States.camera;
    }

    bool switchMix;

    void SwitchMix()
    {
        switchMix = !switchMix;
        if(switchMix)
        {
            mat.SetFloat("_Mixer", .5f);
        }
        else
            mat.SetFloat("_Mixer", 0f);
    }
	// Update is called once per frame
	void Update () {
         
        if( Input.GetKeyUp(KeyCode.N))
        {
            SwitchMix();
        }
        if( Input.GetKeyUp(KeyCode.G) )
        {
            CameraSound.Play();

            Guides.SetActive(false);

            TakeSnapshot();

            //Check if first photo
            if( counter == 1 )
            {
                LoadSigmaColors(PCurrent);
                PAverage.SetPixels(PCurrent.GetPixels());
            }
            counter++;

            state = States.averaging;
            //Store new photo average into PAverage
            Average();

            

            //Morph to average
            if (counter != 2)
            {
                state = States.morphing;
                Morph();
            } 
            else
            {
                SaveTextureToDisk(PAverage);
                state = States.ready;
            }
                

        }

        //Check if we can go to ready mode
        if( Input.GetKeyUp( KeyCode.F ) )
        {
            if( state == States.ready )
            {
                Guides.SetActive(true);
                EnterCameraState();
            }
        }
        
        if( Input.GetKeyUp(KeyCode.V))
        {

            SaveTextureToDisk(PAverage);
        }
    }

    void Morph()
    {
        startTime = Time.time;
       // SetTexture(PAverage);
        mat.SetTexture("_Tex1", PAverage);

        StartCoroutine(MorphAnimation());
    }

    IEnumerator MorphAnimation()
    {
       //while( Time.time - startTime >= )
       
       while(Time.time - startTime <= MorphLength)
       {
            float t = Time.time - startTime;
            mat.SetFloat("_Mixer", map(t, 0, MorphLength, 0, 1) );
            yield return null;
       }
        SetTexture(PAverage);

        mat.SetFloat("_Mixer", 0);

        SaveTextureToDisk(PAverage);

        state = States.ready;
    }

    void SuperImposePrevious()
    {
        mat.SetTexture("_Tex1", PAverage);
        mat.SetFloat("_Mixer", .5f);
    }

    void EnterCameraState()
    {
        mat.SetTexture("_Tex0", PWebCam);
        state = States.camera;
    }

    //Load texture into a Vec4 array
    void LoadSigmaColors(Texture2D tex)
    {
        Color[] colors = tex.GetPixels();
        int i = 0;
        foreach( Color c in colors )
        {    
            PSigmaColors[i++] = new Vector4(c.r, c.g, c.b, c.a);
        }
    }

    //WebCam into PCurrent
    void TakeSnapshot()
    {
        PCurrent.SetPixels(PWebCam.GetPixels());
        PCurrent.Apply();
        SetTexture(PCurrent);
    }
    //PSigma += PCurrent
    void Average()
    {
        Color[] PAveragePixels = PAverage.GetPixels();
        int i = 0;
        foreach( Color c in PCurrent.GetPixels() )
        {
            //Debug.Log(PSigmaPixels[i]);
            float r = PSigmaColors[i][0] + (float)c.r;
            float g = PSigmaColors[i][1] + (float)c.g;
            float b = PSigmaColors[i][2] + (float)c.b;
            float a = PSigmaColors[i][3] + (float)c.a;
            PSigmaColors[i] = new Vector4(r, g, b, a);

            r = r / counter;
            g = g / counter;
            b = b / counter;
            a = a / counter;

            //PSigmaPixels[i] =  PSigmaPixels[i] + c;
            PAveragePixels[i] = new Color(r, g, b, a);
            i++;
        }

       // PSigma.SetPixels(PSigmaPixels);
       // PSigma.Apply();
        PAverage.SetPixels(PAveragePixels);
        PAverage.Apply();
    }


    void SetTexture(Texture2D tex)
    {
        mat.SetTexture("_Tex0", tex);
    }

    void SaveTextureToDisk(Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();
        //Debug.Log(Application.dataPath + "/../Photos/photo-" + counter + "-" + personsName + ".png");
        File.WriteAllBytes( "C:\\Users\\Daniel\\Work\\Erasure\\Photos\\photo_" + counter + "_" + personsName + ".png", bytes);
    }




    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }


    string personsName = "Enter name";
    
    void OnGUI()
    {
        if( state == States.camera)
        personsName = GUI.TextField(new Rect(10, 10, 400, 50), personsName, 25);
    }
    
}
