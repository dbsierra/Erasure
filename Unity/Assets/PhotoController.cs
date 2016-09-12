using UnityEngine;
using System.Collections;

public class SmearStream : Object
{
    private float startTime;
    private Vector2 startPixel;
    private Vector2 endPixel;

    int streamLength;
    
    private Vector2 direction;
    private Texture2D tex;

    private Color[] pixelSnapshot;

    public SmearStream( Vector2 sP, Vector2 eP  )
    {
        startPixel = sP;
        endPixel = eP;
        streamLength = (int)Mathf.Abs(eP.y - sP.y );
        direction = Vector2.up;

        tex = PhotoController.Instance.Texture;
    }
    public void InitStream()
    {
        startTime = Time.time;
    }

    public IEnumerator SmearStreamAnim()
    {
        int r = (int)Random.Range(10, 50);
        float r2 = Random.Range(.1f, .8f);
        float r3 = Random.Range(.8f, 1.5f);

        //get the current pixels

        pixelSnapshot = new Color[(int)endPixel.y];
        int count = 0;
        for (int h = 0; h < (int)endPixel.y; h++)
        {
            pixelSnapshot[count++] = tex.GetPixel((int)startPixel.x, h);
               
        }

        float length = 5f;
        while ( Time.time - startTime < length)
        {
            
            //for every pixel in the stream
            count = 1;
            for( int h=(int)startPixel.y; h < (int)endPixel.y; h++ )
            {
                float lerp = (1 - ((float)(count) / (float)streamLength));

                Color orgC = pixelSnapshot[ h ];
                
                //Color newC = tex.GetPixel( (int)startPixel.x, (int)Mathf.Max(0, h-r) );
                Color newC = pixelSnapshot[ (int)Mathf.Max(0, h - r) ] * new Color(r3, r3, Random.Range(.8f, 1.5f));

                Color fC = Color.Lerp(orgC, newC, ((Time.time - startTime)/ length) * lerp * r2 );

               tex.SetPixel( (int)startPixel.x + (int)Random.Range(0,1), h, fC );

                count++;
            }

            /*
            for (int y = 0; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    Color orgC = PhotoController.Instance.pixels2D[x, y];
                    Color fC = Color.Lerp(orgC, Color.red, Time.time - startTime);

                    PhotoController.Instance.Texture.SetPixel(x, y, fC);
                }
            }*/

            tex.Apply();
            yield return null;
        }
        Done();
    }

    public void Done()
    {
        Object.Destroy( (Object)this );
    }
}


public class PhotoController : MonoBehaviour {

    public Renderer meshRenderer;
    private Material mat;

    public Texture2D tex;


    private byte[] rawData;

    private Color[] pixels;
    public Color[,] pixels2D;   //the original pixels of the image

    private Texture2D texture;
    public Texture2D Texture { get { return texture; } }

    private float time;
    private float timeSnap;

    public static PhotoController Instance;

    //Stream
    private float startTime;
    private Vector2 startPixel;
    private Vector2 endPixel;
    private Vector2 direction;

	// Use this for initialization
	void Start () {
        Instance = this;

        mat = meshRenderer.material;

        texture = new Texture2D(tex.height, tex.width);

        pixels = tex.GetPixels();
        texture.SetPixels(pixels);

        pixels2D = new Color[tex.height, tex.width];
		
		int c = 0;
        //pixels are stored in the 1D array by going down height repeadetly over the width, rather than scanning the width, height number of times
		for( int i=0; i<tex.width; i++ )
		{
			for( int j=0; j<tex.height; j++ )
			{
				pixels2D[ j , i ] = pixels[c++];
			}

		}

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel( x, y, pixels2D[x, y] );
            }
        }

        texture.Apply();

        

        mat.SetTexture("_EmissionMap", texture );


    }

    public static void GetPixels()
    {

    }

    SmearStream s;

    private void Smear()
    {
        StartCoroutine("SmearStreamAnim");
        startTime = time;
    }

    /*
    IEnumerator SmearStreamAnim()
    {
        while( time-startTime < 1 )
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color orgC = pixels2D[x, y];
                    Color fC = Color.Lerp(orgC, Color.red, time - startTime);  
                    texture.SetPixel( x, y, fC);
                }
            }
            texture.Apply();
            yield return null;
        }
    }
	*/

	// Update is called once per frame
	void Update () {

        time += Time.deltaTime;

        if( time - timeSnap >= .01f )
        {
            timeSnap = time;


            float seedX = Mathf.Pow( Random.Range(0f, 1f), 1f);
            float seedY = Mathf.Pow( Random.Range(0f, 1f), 1f);

            float seedHeight = Mathf.Pow(Random.Range(0f, 1f), 10f);


            Vector2 rStart = new Vector2( (int)(seedX * texture.width), (int)(seedY * texture.height));

            Vector2 rEnd = new Vector2(rStart.x, (int)Mathf.Clamp( rStart.y + seedHeight*texture.height + 20, 0, texture.height) );

            //Debug.Log(rStart + " " + rEnd);

            SmearStream sstream = new SmearStream(rStart, rEnd);

            sstream.InitStream();
            StartCoroutine(sstream.SmearStreamAnim());
        }
        
    }
}