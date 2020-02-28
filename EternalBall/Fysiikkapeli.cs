using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Effects;

public class EternalBall : PhysicsGame
{            
    private PhysicsObject pelaaja;
    private PhysicsObject alaReuna;
    private Timer selviytymisAjanSuuruus;
    private List<Timer> lista;
    private IntMeter pelaajanPisteet;
    private Vector nopeusOikealle = new Vector(200, 0);
    private Vector nopeusVasemmalle = new Vector(-200, 0);
    
    private Label pauseViesti;
    private readonly double sade = 40;


    public override void Begin()
    {
        LuoKentta();
        AsetaOhjaimet();
        pelaajanPisteet = LaskePisteet(Level.Left + sade, Level.Top - sade / 2);
        selviytymisAjanSuuruus = LaskeSelviytymisAika();

        lista = new List<Timer>();
        lista.Add(LuoPutoavat(0.8, "vaara"));
        lista.Add(LuoPutoavat(2, "aarre"));
        lista.Add(LuoPutoavat(5, "superolio"));
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
        //Gravity = AsetaTaso(pelaajanPisteet.Value);
        SetWindowSize(600, 750);
        Level.Size = new Vector(600, 750);
        Camera.ZoomToLevel();
        Level.CreateBorders();
        Level.BackgroundColor = Color.White;

        alaReuna = new Surface(Level.Width, sade);
        alaReuna.Position = new Vector(0, Level.Bottom + sade /2);
        Add(alaReuna);

        pelaaja = LuoOlio(this, sade, sade, 0, alaReuna.Top + sade, Color.Blue, null);
        pelaaja.Restitution = 0.0;
        Add(pelaaja);
          
    }
    
    
    /// <summary>
    /// AliOhjelma piirtää ne vaaralliset esineet
    /// </summary>
    private void Piirravaarat()
    {
        
        Image[] pahis = LoadImages("pommi", "bigbomb", "thinbomb", "skeleton", "grenade", "axe");
        PhysicsObject vaara = LuoOlio(this, sade, sade, 
            RandomGen.NextDouble(Level.Left + 5*sade / 2, Level.Right - 5* sade /2),
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
        PhysicsObject aarre = LuoOlio(this, sade, sade,
            RandomGen.NextDouble(Level.Left + 5 * sade / 2, Level.Right - 5 * sade / 2),
            Level.Top,
            Color.Black,
            hyvis[RandomGen.NextInt(hyvis.Length)]);
        this.Add(aarre);
        AddCollisionHandler(aarre, KasitteleAarteidenTormays);
    }


    /// <summary>
    /// AliOhjelma piirtää superolion (treasure box), joka antaa kolme pistettä.
    /// </summary>
    private void PiirraSuperOlio()
    {
        PhysicsObject superOlio = LuoOlio(this, sade, sade,
            RandomGen.NextDouble(Level.Left + 5 * sade / 2, Level.Right - 5 * sade / 2),
            Level.Top, Color.Black,
            LoadImage("treasure"));
        this.Add(superOlio);
        AddCollisionHandler(superOlio, KasitteleSuperOlionTormays);
    }



    /// <summary>
    /// Luodaan 
    /// </summary>
    /// <param name="aika"></param>
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
                aikavali.Timeout += PiirraSuperOlio;
                break;

        }
        aikavali.Start();
        return aikavali;
    }
   

    private void KasitteleAarteidenTormays(PhysicsObject tormaaja, PhysicsObject kohde)

    {
        if (kohde == pelaaja)
        {
            pelaajanPisteet.Value += 1;
            Gravity = AsetaTaso(pelaajanPisteet.Value);
            tormaaja.Destroy();
        }
        if(kohde == alaReuna) tormaaja.Destroy();
    }


    /// <summary>
    /// Käsitellään aliohjelmassa superolion törmäystilanteita.
    /// </summary>
    /// <param name="tormaaja"></param>
    /// <param name="kohde"></param>
    private void KasitteleSuperOlionTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        if (kohde == pelaaja)
        {
            tormaaja.Destroy();
            pelaajanPisteet.Value += 3;
            
        }
        if (kohde == alaReuna) tormaaja.Destroy();
    }
    private void AsetaOhjaimet()
    {
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, null, pelaaja, nopeusOikealle);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, null, pelaaja, nopeusVasemmalle);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopettaa peli");
        Keyboard.Listen(Key.P, ButtonState.Pressed, LaitaPauselle, "Laittaa pelin pauselle");
    }


    private void AsetaNopeus(PhysicsObject pallo, Vector nopeus)
    {
        pallo.Velocity = nopeus;

    }
    

/// <summary>
/// Lasketaan, kuinka kauan pelaaja selviää pelissä.
/// </summary>
/// <returns>selviytymisaika</returns>
    private Timer LaskeSelviytymisAika()
    {
        Timer selviytymisAika = new Timer();
        

        Label piste = new Label();
        piste.TextColor = Color.Yellow;
        piste.Color = Color.Black;
        piste.BorderColor = Color.White;
        piste.Size = new Vector(sade, sade);
        piste.BindTo(selviytymisAika.SecondCounter);
        piste.X = Level.Right - sade;
        piste.Y = Level.Top - sade / 2;
        piste.DecimalPlaces =2;
        Add(piste);
        selviytymisAika.Start();
        return selviytymisAika;
    }


/// <summary>
    /// Aliohjelma käsittelee vaarallisten esineiden törmäystilanteita. 
    /// </summary>
    /// <param name="tormaaja">Objekti, jonka käsittelystä ollaan kiinnostuneita</param>
    /// <param name="kohde">Objekti, johon törmäys kohdistuu</param>
    private void KasitteleVaaranTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        
        if (kohde == pelaaja)
        {
            Explosion boom = new Explosion(tormaaja.Width);
            Add(boom);
            kohde.Destroy();
            tormaaja.Destroy();
            for (int i = 0; i < lista.Count;  i++)
            {
                lista[i].Stop();
            }
            selviytymisAjanSuuruus.Stop();
            GameOverViesti();
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaAlusta, "aloittaa uudestaan");
        }

        if (kohde == alaReuna) tormaaja.Destroy();
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
        pistenaytto.Size = new Vector(20, 20);
        pistenaytto.TextColor = Color.Yellow;
        pistenaytto.X = x;
        pistenaytto.Y = y;
        pistenaytto.Color = Color.Black;
        pistenaytto.BindTo(pisteLasku);
        Add(pistenaytto);

        return pisteLasku;
    }

                    
/// <summary>
/// Luodaan näytölle viesti, kun pelaajan häviää ja näytetään hänen selviytymisajan
/// suuruus ja kerättyjen pisteiden määrä
/// </summary>
    private void GameOverViesti()
    {
        Label gameOver = new Label("Game Over \n" + "your score: " + pelaajanPisteet.Value + "\n" + "Your survival time: "
            + selviytymisAjanSuuruus.CurrentTime.ToString("F2") + " s \n" + "Press Enter to continue");
        gameOver.Position = new Vector(0, 0);
        gameOver.BorderColor = Color.Black;
        gameOver.Color = Color.BrightGreen;
        gameOver.TextColor = Color.Black;
        Add(gameOver);
    }


/// <summary>
/// Aliohjelma laittaa pelin pauselle
/// </summary>
    private void LaitaPauselle()
    {
        
        if (IsPaused == true)
        {
            IsPaused = false;
            pauseViesti.Destroy();
        }
        else if ( pelaaja.IsDestroyed == true) IsPaused = false;
        else {
            IsPaused = true;
            pauseViesti = new Label("Game is Paused. \nPress 'P' to continue");
            pauseViesti.Position = new Vector(0, 0);
            pauseViesti.BorderColor = Color.Black;
            pauseViesti.Color = Color.BrightGreen;
            Add(pauseViesti);
        }
    }


    private Vector AsetaTaso(int pelaajanpiste)
    {
        Gravity = new Vector(0, 0);
        pelaajanpiste = pelaajanPisteet.Value;
        if (pelaajanpiste < 10)
        {
            Gravity = new Vector(0, -200);
        }
        else Gravity = new Vector(0, -1000);
        return Gravity;
    }



/// <summary>
/// Tämä aliohjelma aloittaa pelin uudestaan.
/// </summary>
    private void AloitaAlusta()
    {
        ClearAll(); // poistaa 
        LuoKentta();
        AsetaOhjaimet();
        pelaajanPisteet = LaskePisteet(Level.Left + sade, Level.Top - sade / 2);
        selviytymisAjanSuuruus = LaskeSelviytymisAika();

        lista = new List<Timer>();
        lista.Add(LuoPutoavat(0.8, "vaara"));
        lista.Add(LuoPutoavat(2, "aarre"));
        lista.Add(LuoPutoavat(5, "superolio"));

        Gravity = new Vector(0, -200);
    }
}