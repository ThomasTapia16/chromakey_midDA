using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using SimpleJSON;
using System.IO;
using UnityEngine.Video;

public class ListaPostales : MonoBehaviour
{
    public Image image;
    public GameObject panel;
    public string Estado;
    private TextMeshProUGUI Temp;
    public int contador = 0;
    public Sprite[] Capturas;
    public SpriteRenderer fondo;
    private float tiempo = 10f;
    private bool habilitarTemp = false;
    public Enviar enviar;
    private string area_tematica;
    public InputField Para;
    private string Adjunto;
    public RenderTexture ScreenShot;

    //para mostrar videos
    public VideoClip[] videos;
    public int contadorV = 0;
    public VideoPlayer vid;
    public RawImage rawvideo;
    //fonod video
    public VideoPlayer videoF;
    public SpriteRenderer fondov;

    void Start()
    {   //para mostrar video
        this.vid = GameObject.Find("rawImage").GetComponent<VideoPlayer>();
        this.rawvideo = GameObject.Find("rawImage").GetComponent<RawImage>();
        this.rawvideo.enabled = false;
        //fondo video
        this.videoF = GameObject.Find("fondoV").GetComponent<VideoPlayer>();
        this.fondov = GameObject.Find("fondoV").GetComponent<SpriteRenderer>();
        //---------------------------------------------------------------------------------------
        this.fondov.enabled = false;
        this.Temp = GameObject.Find("Temporizador").GetComponent<TextMeshProUGUI>();
        this.Temp.enabled = false;
        this.fondo = GameObject.Find("Fondo").GetComponent<SpriteRenderer>();
        this.image.sprite = this.Capturas[contador];
        this.panel.SetActive(false);
        this.Estado = "Oculto";
        StartCoroutine(obtenerCategoria());
        this.fondo.sprite = this.Capturas[contador];

        
       
    }

    public IEnumerator obtenerCategoria()
    {
        UnityWebRequest categoria = UnityWebRequest.Get("http://200.14.71.62/web_midda_admin/public/api/configuracion/3");
        yield return categoria.SendWebRequest();
        if (categoria.isNetworkError || categoria.isHttpError)
        {
            Debug.Log(categoria.error);
        }
        else
        {
            Debug.Log(categoria.downloadHandler.text);
            JSONNode data = JSON.Parse(categoria.downloadHandler.text);
            Debug.Log("Categoria = " + data["area_tematica"]);
            this.Capturas = Resources.LoadAll<Sprite>("Paisajes/" + data["area_tematica"]);
            //arreglo con los videos disponibles
            this.videos = Resources.LoadAll<VideoClip>("Paisajes/" + data["area_tematica"]);
        }
    }

    void Update()
    {
        
        if (this.habilitarTemp == true)
        {
            this.tiempo -= Time.deltaTime;
            this.Temp.text = " " + this.tiempo.ToString("f0");
            if (this.tiempo <= 1)
            {
                this.Temp.enabled = false;
                this.habilitarTemp = false;
                capturarImagen();
            }
        }
    }

    public void lanzarTemporizador()
    {
        this.panel.SetActive(false);
        this.Temp.enabled = true;
        this.tiempo = 10f;
        this.Temp.text = "" + tiempo;
        this.habilitarTemp = true;
    }


    public void mostrarConfiguracion()
    {
        
        if (this.Estado == "Oculto")
        {
            
            this.panel.SetActive(true);
            this.Estado = "Visible";
        }
        else
        {
           
            this.panel.SetActive(false);
            this.Estado = "Oculto";
        }
    }

    public void capturarImagen()
    {
        Debug.Log("Captura Foto");
        /*string screenshotIMGName = System.DateTime.Now.ToString();
        string subString = screenshotIMGName.Replace('/', '_');
        string gypsy = subString.Replace(':', '_');
        string NombreFinal = gypsy + ".png";
        ScreenCapture.CaptureScreenshot(NombreFinal);
        mensajeCaptura(NombreFinal);*/
        StartCoroutine(SaveSS());


    }

    public void mensajeCaptura(string NombreCaptura)
    {
        //Debug.Log("Se capturo la imagen con el nombre de "+NombreCaptura);
        //this.enviar.sendEmail(NombreCaptura);
        //this.Adjunto = @"D:\Proyectos-unity\chromakey\"+NombreCaptura;
    }

    public void videoP()
    {   
        if (this.contadorV >= videos.Length)
        {
            this.image.enabled = true;
            this.rawvideo.enabled = false;
            this.fondov.enabled = false;
            vid.Stop();
            videoF.Stop();
            this.contadorV = 0;
            this.contador = 0;
        }

        else
        {
            this.fondo.enabled = false;
            this.image.enabled = false;
            this.fondov.enabled = true;
            this.rawvideo.enabled = true;
            this.vid.clip = videos[this.contadorV];
            this.videoF.clip = videos[this.contadorV];
            vid.Play();
            videoF.Play();
            this.contadorV++;

        }



    }
    public void cambiarImagenP()
    {
        Debug.Log("estas eb cambiarImagenP");
        this.fondo.enabled = true;
        this.image.enabled = true;
        if (this.contador < Capturas.Length)
        {
            
            this.rawvideo.enabled = false;
            this.image.sprite = this.Capturas[contador];

            this.fondo.sprite = this.image.sprite;
            this.contador++;
        }
        else if (this.contador == Capturas.Length)
        {
            videoP();
            
        }
    }
 
    public void cambiarImagenA()
    {
        this.contador--;
        if (this.contador < 0)
        {
            Debug.Log(contador);
            this.contador = this.Capturas.Length;
            this.contador--;
        }
        
        this.image.sprite = this.Capturas[contador];
        this.fondo.sprite = this.image.sprite;
       
    }

    IEnumerator SaveSS() //corrutina que guarda el screenshot
    {
        Debug.Log("Comienza Proceso");
        //GameObject.Find("TextEstadoHttp").GetComponent<Text>().enabled=true;
        yield return new WaitForEndOfFrame(); //espera al ultimo frame
        Debug.Log(Application.dataPath); //muestra donde se guarda el SS

        RenderTexture.active = ScreenShot; //componente que lleva la captura

        var texture2D = new Texture2D(ScreenShot.width, ScreenShot.height); //Crea una textura 2d y le añade alto y ancho del render.
        texture2D.ReadPixels(new Rect(0, 0, ScreenShot.width, ScreenShot.height), 0, 0);// convierte la textura a pixeles?
        texture2D.Apply();

        var data = texture2D.EncodeToPNG(); //guarda la textura 2d como un png

        File.WriteAllBytes(Application.dataPath + "/ScreenShots/savedImage.png", data); //guarda la captura en un directorio dentro de la carpeta asset del proyecto.

        //ENVIAR CORREO
        Debug.Log("Comienza Envio de Correo a " + this.Para.text);
        WWWForm form = new WWWForm();
        form.AddField("correo", "matiasliv@gmail.com");
        form.AddBinaryData("foto", data, "screen.jpeg", "image/jpeg");

        using (UnityWebRequest uwr = UnityWebRequest.Post("http://200.14.71.62/web_midda_admin/public/enviarCorreo", form))
        {
            yield return uwr.SendWebRequest();


            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                //GameObject.Find("TextEstadoHttp").GetComponent<Text>().text="Envio completado";
            }

        }


    }



}