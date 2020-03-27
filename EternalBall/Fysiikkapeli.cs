using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;


/// <summary>
/// Tehdään pallo peli EternalBall, 
/// </summary>
public class EternalBall : PhysicsGame
{            
    private PhysicsObject PELAAJA;
    private PhysicsObject ALAREUNA;
    private Timer SELVIYTYMISAIKA;
    private List<Timer> LISTA;
    private IntMeter PELAAJANPISTEET;
    private Label PAUSEVIESTI;
    private readonly double SADE = 40;
    


    public override void Begin()
    {       
        LuoKentta();
        AsetaOhjaimet();
        PELAAJANPISTEET = LaskePisteet(Level.Left + SADE / 2, Level.Top - SADE / 2);
        SELVIYTYMISAIKA = LaskeSelviytymisAika();
        LISTA = new List<Timer>();
        LISTA.Add(LuoPutoavat(0.8, "vaara"));
        LISTA.Add(LuoPutoavat(2, "aarre"));
        LISTA.Add(LuoPutoavat(5, "superolio"));
        Gravity = new Vector(0, -200);
     }


    private static PhysicsObject LuoOlio(Game peli, double olionLeveys, double olionPituus,
        double x, double y, Color vari, Image kuva )
    {
        PhysicsObject olio = new PhysicsObject(olionLeveys, olionPituus, Shape.Circle);
        olio.Y = y;
        olio.X = x;
        olio.Color = vari;
        olio.Image = kuva;
        peli.Add(olio);
        return olio;
    }

    /// <summary>
    /// Luodaan pelikenttä, johon kuuluu pelaajan ohjattavissa oleva pallo, alareuna, 
    /// seinät ja pelikentän asetukset mm. painovoima, pelikentän suuruus, taustaväri ...
    /// </summary>
    private void LuoKentta()
    {
        
        SetWindowSize(600, 750);
        Level.Size = new Vector(600, 750);
        Camera.ZoomToLevel();
        Level.CreateBorders();
        Level.BackgroundColor = Color.White;
        MediaPlayer.Play("Stratosphere_Looping");
        MediaPlayer.IsRepeating = true;

        ALAREUNA = new Surface(Level.Width, SADE);
        ALAREUNA.Position = new Vector(0, Level.Bottom + SADE /2);
        Add(ALAREUNA);

        PELAAJA = LuoOlio(this, SADE, SADE, 0, ALAREUNA.Top + SADE, Color.Blue, null);
        PELAAJA.Restitution = 0.0;
        Add(PELAAJA);
          
    }
    
    
    /// <summary>
    /// AliOhjelma piirtää ne vaaralliset esineet
    /// </summary>
    private void Piirravaarat()
    {
        
        Image[] pahis = LoadImages("pommi", "bigbomb", "thinbomb", "skeleton", "grenade", "axe");
        PhysicsObject vaara = LuoOlio(this, SADE, SADE, 
            RandomGen.NextDouble(Level.Left + 5 * SADE / 2, Level.Right - 5 * SADE / 2),
            Level.Top, Color.Black, pahis[RandomGen.NextInt(pahis.Length)]);
        this.Add(vaara);
        AddCollisionHandler(vaara, KasitteleVaaranTormays);
    }


    /// <summary>
    /// Aliohjelma piirtää aarteet.
    /// </summary>
    private void PiirraAarteet()
    {
        Image[] hyvis = LoadImages("diamondblue", "goldcoin", "ruby");
        PhysicsObject aarre = LuoOlio(this, SADE, SADE,
            RandomGen.NextDouble(Level.Left + 5 * SADE / 2, Level.Right - 5 * SADE / 2),
            Level.Top,
            Color.Black,
            hyvis[RandomGen.NextInt(hyvis.Length)]);
        this.Add(aarre);
        AddCollisionHandler(aarre, KasitteleAarteidenTormays);
    }


    /// <summary>
    /// AliOhjelma piirtää aarrelaatikon (treasure box), joka antaa kolme pistettä.
    /// </summary>
    private void PiirraAarreLaatikko()
    {
        PhysicsObject superOlio = LuoOlio(this, SADE, SADE,
            RandomGen.NextDouble(Level.Left + 5 * SADE / 2, Level.Right - 5 * SADE / 2),
            Level.Top, Color.Black,
            LoadImage("treasure"));
        this.Add(superOlio);
        AddCollisionHandler(superOlio, KasitteleAarreLaatikonTormays);
    }


    /// <summary>
    /// Luodaan putoavia esineitä tietyillä aikaväleillä.
    /// </summary>
    /// <param name="aika">se aika, jonka </param>
    /// <param name="tyyppi">1 on vaarat 2 on aarteet</param>
    private Timer LuoPutoavat(double aika, string tyyppi)
    {
        Timer aikavali = new Timer();
        aikavali.Interval = aika;
        switch(tyyppi)
        {      
            case "vaara":
                aikavali.Timeout += Piirravaarat;      
                break;
            case "aarre":
                aikavali.Timeout += PiirraAarteet;
                break;
            case "superolio":
                aikavali.Timeout += PiirraAarreLaatikko;
                break;
        }
        aikavali.Start();
        return aikavali;
    }

    /// <summary>
    /// Aliohjelma käsittelee vaarallisten esineiden törmäystilanteita. 
    /// </summary>
    /// <param name="tormaaja">Objekti, jonka käsittelystä ollaan kiinnostuneita</param>
    /// <param name="kohde">Objekti, johon törmäys kohdistuu</param>
    private void KasitteleVaaranTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        if (kohde == PELAAJA)
        {
            Explosion rajahdys = new Explosion(tormaaja.Width);
            Add(rajahdys);
            kohde.Destroy();
            tormaaja.Destroy();
            for (int i = 0; i < LISTA.Count; i++)
            {
                LISTA[i].Stop();
            }
            SELVIYTYMISAIKA.Stop();
            GameOverViesti();
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate () {
                ClearAll();
                Begin();
            }, "aloittaa uudestaan");
        }
        if (kohde == ALAREUNA) tormaaja.Destroy();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="tormaaja"></param>
    /// <param name="kohde"></param>
    private void KasitteleAarteidenTormays(PhysicsObject tormaaja, PhysicsObject kohde)

    {
        if (kohde == PELAAJA)
        {
            double tasoKerroin = 200;
            PELAAJANPISTEET.Value += 1;
            Gravity = new Vector(0, -tasoKerroin -tasoKerroin * (PELAAJANPISTEET.Value / 20)); 
            tormaaja.Destroy();
        }
        if(kohde == ALAREUNA) tormaaja.Destroy();
    }


    /// <summary>
    /// Käsitellään aliohjelmassa aarrelaatikon törmäystilanteita.
    /// </summary>
    /// <param name="tormaaja">Objekti, jonka törmäyksestä ollaan kiinnostuneita.</param>
    /// <param name="kohde"> Objekti, johon törmätään</param>
    private void KasitteleAarreLaatikonTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        if (kohde == PELAAJA)
        {
            tormaaja.Destroy();
            PELAAJANPISTEET.Value += 3;            
        }
        if (kohde == ALAREUNA) tormaaja.Destroy();
    }
   

    /// <summary>
    /// Asetetaan ohjaimet: Right-näppäin vie pelaajaa oikealle
    /// Left-näppäin vie pelaajaa vasemmalle
    /// </summary>
    private void AsetaOhjaimet()
    {      
        Vector nopeusOikealle = new Vector(300, 0);
        Vector nopeusVasemmalle = new Vector(-300, 0);       
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, null, PELAAJA, nopeusOikealle);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, PELAAJA, Vector.Zero);
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, null, PELAAJA, nopeusVasemmalle);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, PELAAJA, Vector.Zero);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopettaa peli");
        Keyboard.Listen(Key.P, ButtonState.Pressed, delegate()
        {
            if (IsPaused == true)
            {
                IsPaused = false;
                PAUSEVIESTI.Destroy();
            }
            else if (PELAAJA.IsDestroyed == true) IsPaused = false;
            else
            {
                IsPaused = true;
                PAUSEVIESTI = new Label("Game is Paused. \nPress 'P' to continue");
                PAUSEVIESTI.Position = new Vector(0, 0);
                PAUSEVIESTI.BorderColor = Color.Black;
                PAUSEVIESTI.Color = Color.BrightGreen;
                Add(PAUSEVIESTI);
            }
        }, "Laittaa peliä pauselle");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="pallo"></param>
    /// <param name="nopeus"></param>
    private void AsetaNopeus(PhysicsObject pallo, Vector nopeus)
    {
        pallo.Velocity = nopeus;
    }
    

/// <summary>
/// Lasketaan, kuinka kauan PELAAJA selviää pelissä.
/// </summary>
/// <returns>selviytymisaika</returns>
    private Timer LaskeSelviytymisAika()
    {
        Timer selviytymisAika = new Timer();
        Label piste = new Label();
        piste.TextColor = Color.Yellow;
        piste.Color = Color.Black;
        piste.BorderColor = Color.White;
        piste.Size = new Vector(SADE, SADE);
        piste.BindTo(selviytymisAika.SecondCounter);
        piste.X = Level.Right - SADE;
        piste.Y = Level.Top - SADE / 2;
        piste.DecimalPlaces =2;
        Add(piste);
        selviytymisAika.Start();
        return selviytymisAika;
    }


/// <summary>
    /// Luodaan pistelaskuri ja sen näyttö.
    /// </summary>
    /// <param name="x">pistelaskurin paikka pelialueella, sen x-koordinaatti</param>
    /// <param name="y">pistelaskurin paikka pelialueella, sen y-koordinaatti</param>
    /// <returns></returns>
    private IntMeter LaskePisteet(double x, double y)
    {
        IntMeter pisteLasku = new IntMeter(0);
        Label pistenaytto = new Label();
        pistenaytto.Size = new Vector(SADE, SADE);
        pistenaytto.TextColor = Color.Yellow;
        pistenaytto.Position = new Vector(x, y);
        pistenaytto.Color = Color.Black;
        pistenaytto.BindTo(pisteLasku);
        Add(pistenaytto);
        return pisteLasku;
    }

                    
/// <summary>
/// Luodaan näytölle viesti, kun pelaaja häviää ja näytetään hänen selviytymisajan
/// suuruus ja kerättyjen pisteiden määrä
/// </summary>
    private void GameOverViesti()
    {
        Label gameOver = new Label("Game Over \n" + "your score: " + PELAAJANPISTEET.Value + "\n" + "Your survival time: "
            + SELVIYTYMISAIKA.CurrentTime.ToString("F2") + " s \n" + "Press Enter to continue");
        gameOver.BorderColor = Color.Black;
        gameOver.Color = Color.BrightGreen;
        gameOver.TextColor = Color.Black;
        Add(gameOver);
    }
     
}