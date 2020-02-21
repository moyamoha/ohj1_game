using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class EternalBall : PhysicsGame
{

    
                 
    private PhysicsObject pelaaja;
    private PhysicsObject p ;
   // private PhysicsObject syotava;
    private PhysicsObject alaReuna;
    private Timer selviytymisAjanSuuruus;
    private Timer[] lista;
    private PhysicsObject seina;
    private IntMeter pelaajanPisteet;
    private Vector nopeusOikealle = new Vector(200, 0);
    private Vector nopeusVasemmalle = new Vector(-200, 0);
    private const double sade = 30;
    

    //Vector painoVoima;

    public override void Begin()
    {
        LuoKentta();
        LisaaLaskurit();
        AsetaOhjaimet();
        lista = new Timer[] {
            LuoPutoavat(1,"vaara"),
            LuoPutoavat(1, "aarre")
        };
        
        
       
        
    }


    private static PhysicsObject LuoOlio(Game peli, double olionLeveys, double olionPituus, double x, double y)
    {
        PhysicsObject olio = new PhysicsObject(olionLeveys, olionPituus, Shape.Circle);
        olio.Y = y;
        olio.X = x;
        olio.Color = Color.White;
        peli.Add(olio);
        return olio;
    }


    private void LuoKentta()
    {


        Gravity = new Vector(0, -200);
        SetWindowSize(600, 750);
        Level.Size = new Vector(600, 750);
        Camera.ZoomToLevel();
        Level.CreateBorders();
        Level.BackgroundColor = Color.White;

        alaReuna = Level.CreateBottomBorder();
        alaReuna.Restitution = 1.0;
        alaReuna.IsVisible = false;
        alaReuna.KineticFriction = 0.0;
        Level.CreateBorders();

        LuoSeina(Level.Right, 0, Level.Height, 50);
        LuoSeina(Level.Left, 0, Level.Height, 50);

        
        pelaaja = LuoOlio(this, sade, sade, 0, alaReuna.Top + sade);
        pelaaja.Color = Color.Blue;
        pelaaja.Restitution = 0.0;
        this.Add(pelaaja);
        
        
    }

    

    

    private void Piirravaarat()
    {
        
        Image[] pahis = LoadImages("pommi", "bigbomb", "thinbomb", "skeleton", "grenade", "axe");
        p = LuoOlio(this, sade, sade, RandomGen.NextDouble(Level.Left + seina.Width, Level.Right - seina.Width), Level.Top);
        p.Restitution = 0;
        p.Image = pahis[RandomGen.NextInt(pahis.Length)];
        
        this.Add(p);
        AddCollisionHandler(this.p, KasitteleTormays);
    }

    private void PiirraAarteet()
    {
        const double sade = 30;
        Image[] hyvis = LoadImages("heart", "diamondblue", "goldcoin", "ruby");
        PhysicsObject syotava = LuoOlio(this, sade, sade, RandomGen.NextDouble(Level.Left + seina.Width + sade, Level.Right - seina.Width - sade), Level.Top);
        syotava.Image = hyvis[RandomGen.NextInt(hyvis.Length)];
        syotava.Restitution = 0;
        syotava.Tag = "syotava";
        this.Add(syotava);
        AddCollisionHandler(syotava, NappaaAarteet);


    }

    /// <summary>
    /// 
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
        }
        aikavali.Start();
        return aikavali;
    }
   

    private void NappaaAarteet(PhysicsObject tormaaja, PhysicsObject kohde)

    {
        if (kohde == pelaaja)
        {
            pelaajanPisteet.Value += 1;
            tormaaja.Destroy();
        }
        if(kohde == alaReuna) tormaaja.Destroy();
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
/// Lasketaan, kuinka pitkällä pelaaja selviää pelissä.
/// </summary>
/// <returns>selviytymisaika</returns>
    private Timer LaskeSelviytymisAika()
    {
        Timer selviytymisAika = new Timer();
        

        Label piste = new Label();
        piste.TextColor = Color.Yellow;
        piste.Color = Color.Black;
        piste.BorderColor = Color.White;
        piste.Size = new Vector(20, 20);
        piste.BindTo(selviytymisAika.SecondCounter);
        piste.X = Level.Right - 20;
        piste.Y = Level.Top - 20;
        Add(piste);
        selviytymisAika.Start();
        
        return selviytymisAika;
    }


/// <summary>
    /// Aliohjelma käsittelee törmäyksia. 
    /// </summary>
    /// <param name="tormaaja">Objekti, jonka käsittelystä ollaan kiinnostuneita</param>
    /// <param name="kohde">Objekti, johon törmäys kohdistuu</param>
    private void KasitteleTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        
        if (kohde == pelaaja)
        {
            Explosion boom = new Explosion(tormaaja.Width);
            Add(boom);
            kohde.Destroy();
            tormaaja.Destroy();
            //selviytymisAika.Stop();
            for (int i = 0; i < lista.Length;  i++)
            {
                lista[i].Stop();
            }
            selviytymisAjanSuuruus.Pause();
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
    /// Lisätään laskurit peliin, joita on selviytymislaskuri ja pistelaskuri
    /// </summary>
    private void LisaaLaskurit()
    {
        pelaajanPisteet = LaskePisteet(Level.Left + 10, Level.Top - 10);
        selviytymisAjanSuuruus = LaskeSelviytymisAika();
    }


    /// <summary>
    /// Luodaan pelialueen molemmille puolelle seinät. 
    /// </summary>
    /// <param name="x">seinän x-koordinaatti</param>
    /// <param name="y">seinä y-koordinaatti</param>
    /// <param name="pituus">seinän pituus</param>
    /// <param name="leveys">seinän levey</param>
    private void LuoSeina (double x, double y, double pituus, double leveys)
    {
        seina = PhysicsObject.CreateStaticObject (leveys, pituus, Shape.Rectangle);
        seina.X = x;
        seina.Y = y;
        seina.Color = Color.Azure;
        seina.Image = LoadImage("wall");
        Add(seina);
    }


    

/// <summary>
/// Luodaan näytölle viesti, kun pelaajan häviää ja näytetään hänen selviytymisajan
/// suuruus ja kerättyjen pisteiden määrä
/// </summary>
    private void GameOverViesti()
    {
        Label gameOver = new Label("Game Over \n" + "your score: " + pelaajanPisteet.Value + "\n" + "Your survival time: "
            + selviytymisAjanSuuruus.CurrentTime + "\n" + "Press Enter to continue");
        gameOver.Position = new Vector(0, 0);
        gameOver.BorderColor = Color.Black;
        gameOver.Color = Color.BrightGreen;
        gameOver.TextColor = Color.Black;
        gameOver.LifetimeLeft = TimeSpan.FromSeconds(15);

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
        }
        else IsPaused = true;

    }

/// <summary>
/// Tämä aliohjelma aloittaa pelin uudestaan.
/// </summary>
    private void AloitaAlusta()
    {
        ClearAll(); // poistaa 
        LuoKentta();
        lista = new Timer[] {
            LuoPutoavat(1,"vaara"),
            LuoPutoavat(1, "aarre")
        };
        LisaaLaskurit();
        AsetaOhjaimet();
        
    }
}