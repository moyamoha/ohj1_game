using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;

/// @author  Mohamad Yahya Mohammad Javad
/// @version 04.2020
/// <summary>
/// Tehdään pallopeli EternalBall. Pelissä on tarkoitus kerätä pisteitä ja väistää vaaroja niin 
/// että selviää mahdollisimman pitkälle. Pelissä on myös vaikeustasoja suhteessa pelaajan kerättyihin pisteisiin. 
/// </summary>
public class EternalBall : PhysicsGame
{            
    private PhysicsObject pelaaja;
    private PhysicsObject alareuna;
    private Timer selviytymisaika;
    private List<Timer> luoOliotLista;
    private IntMeter pelaajanPisteet;
    private Label pysahtymisViesti;
    private readonly double olionMitta = 40;
    

    /// <summary>
    /// Peli alkaa tästä aliohjelmasta, jossa kutsutaan muita aliohjelmia/funktioita
    /// </summary>
    public override void Begin()
    {       
        LuoKentta();
        AsetaOhjaimet();
        pelaajanPisteet = LaskePisteet(Level.Left + olionMitta / 2, Level.Top - olionMitta / 2);
        selviytymisaika = LaskeSelviytymisAika();
        luoOliotLista = new List<Timer>();
        luoOliotLista.Add(LuoPutoavat(0.8, "vaara"));
        luoOliotLista.Add(LuoPutoavat(2, "aarre"));
        luoOliotLista.Add(LuoPutoavat(5, "superolio"));
        Gravity = new Vector(0, -200);
     }


    /// <summary>
    /// Luodaan pohja pelissä olevia esineita varten
    /// </summary>
    /// <param name="peli">Tämä peli</param>
    /// <param name="olionLeveys">objektin leveys</param>
    /// <param name="olionPituus">objektin pituus</param>
    /// <param name="x">objektin x-koordinaatti</param>
    /// <param name="y">objektin y-koordinaatti</param>
    /// <param name="vari">objektin väri</param>
    /// <param name="kuva">objektiin liitetty kuva</param>
    /// <returns> olio</returns>
    private PhysicsObject LuoOlio(Game peli, double olionLeveys, double olionPituus,
        double x, double y, Color vari, Image kuva )
    {
        PhysicsObject olio = new PhysicsObject(olionLeveys, olionPituus, Shape.Circle);
        olio.Y = y;
        olio.X = x;
        olio.Color = vari;
        olio.Image = kuva;
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
        // Luodaan alareuna
        alareuna = new Surface(Level.Width, olionMitta);
        alareuna.Position = new Vector(0, Level.Bottom + olionMitta /2);
        Add(alareuna);
        // Luodaan pelaaja
        pelaaja = LuoOlio(this, olionMitta, olionMitta, 0, alareuna.Top + olionMitta, Color.Blue, null);
        pelaaja.Restitution = 0.0;
        Add(pelaaja);          
    }
    
    
    /// <summary>
    /// AliOhjelma piirtää ne vaaralliset esineet
    /// </summary>
    private void Piirravaarat()
    {        
        Image[] pahis = LoadImages("pommi", "bigbomb", "thinbomb", "skeleton", "grenade", "axe");
        PhysicsObject vaara = LuoOlio(this, olionMitta, olionMitta, 
            RandomGen.NextDouble(Level.Left + 5 * olionMitta / 2, Level.Right - 5 * olionMitta / 2),
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
        PhysicsObject aarre = LuoOlio(this, olionMitta, olionMitta,
            RandomGen.NextDouble(Level.Left + 5 * olionMitta / 2, Level.Right - 5 * olionMitta / 2),
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
        PhysicsObject superOlio = LuoOlio(this, olionMitta, olionMitta,
            RandomGen.NextDouble(Level.Left + 5 * olionMitta / 2, Level.Right - 5 * olionMitta / 2),
            Level.Top, Color.Black,
            LoadImage("treasure"));
        this.Add(superOlio);
        AddCollisionHandler(superOlio, KasitteleAarreLaatikonTormays);
    }


    /// <summary>
    /// Luodaan putoavia esineitä tietyillä aikaväleillä.
    /// </summary>
    /// <param name="aika">aikaväli </param>
    /// <param name="tyyppi">luotavan esineen tyyppi merkkijonona</param>
    /// <returns>aikaväli double-lukuna</returns>
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
        if (kohde == pelaaja)
        {
            Explosion rajahdys = new Explosion(tormaaja.Width);
            Add(rajahdys);
            kohde.Destroy();
            tormaaja.Destroy();
            for (int i = 0; i < luoOliotLista.Count; i++)
            {
                luoOliotLista[i].Stop();
            }
            selviytymisaika.Stop();
            NaytaGameOverViesti();
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate () {
                ClearAll();
                Begin();
            }, "aloittaa uudestaan");
        }
        if (kohde == alareuna) tormaaja.Destroy();
    }


    /// <summary>
    /// Käsitellään kerättävien esineiden törmäystilanteita.
    /// </summary>
    /// <param name="tormaaja">Törmääjä (aarre)</param>
    /// <param name="kohde">kohde, johon törmätään (pelaaja tai alareuna)</param>
    private void KasitteleAarteidenTormays(PhysicsObject tormaaja, PhysicsObject kohde)

    {
        if (kohde == pelaaja)
        {
            double tasoKerroin = 200;
            pelaajanPisteet.Value += 1;
            Gravity = new Vector(0, -tasoKerroin -tasoKerroin * (pelaajanPisteet.Value / 20)); 
            tormaaja.Destroy();
        }
        if(kohde == alareuna) tormaaja.Destroy();
    }


    /// <summary>
    /// Käsitellään aliohjelmassa aarrelaatikon törmäystilanteita.
    /// </summary>
    /// <param name="tormaaja">Objekti, jonka törmäyksestä ollaan kiinnostuneita.</param>
    /// <param name="kohde"> Objekti, johon törmätään</param>
    private void KasitteleAarreLaatikonTormays(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        if (kohde == pelaaja)
        {
            tormaaja.Destroy();
            pelaajanPisteet.Value += 3;            
        }
        if (kohde == alareuna) tormaaja.Destroy();
    }
   

    /// <summary>
    /// Asetetaan ohjaimet: Right-näppäin vie pelaajaa oikealle
    /// Left-näppäin vie pelaajaa vasemmalle
    /// </summary>
    private void AsetaOhjaimet()
    {      
        Vector nopeusOikealle = new Vector(300, 0);
        Vector nopeusVasemmalle = new Vector(-300, 0);       
        Keyboard.Listen(Key.Right, ButtonState.Down, AsetaNopeus, null, pelaaja, nopeusOikealle);
        Keyboard.Listen(Key.Right, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Left, ButtonState.Down, AsetaNopeus, null, pelaaja, nopeusVasemmalle);
        Keyboard.Listen(Key.Left, ButtonState.Released, AsetaNopeus, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopettaa peli");
        Keyboard.Listen(Key.P, ButtonState.Pressed, delegate()
        {
            if (IsPaused == true)
            {
                IsPaused = false;
                pysahtymisViesti.Destroy();
                MediaPlayer.Resume();
            }
            else if (pelaaja.IsDestroyed == true) IsPaused = false;
            else
            {
                IsPaused = true;
                MediaPlayer.Pause();
                pysahtymisViesti = new Label("Game is Paused. \nPress 'P' to continue");
                pysahtymisViesti.Position = new Vector(0, 0);
                pysahtymisViesti.BorderColor = Color.Black;
                pysahtymisViesti.Color = Color.BrightGreen;
                Add(pysahtymisViesti);
            }
        }, "Laittaa peliä pauselle");
    }


    /// <summary>
    /// Asetetaan PELAAJALLE nopeus
    /// </summary>
    /// <param name="pallo">objekti, jonka nopeus säädellään (tässä tapauksessa pelaaja)</param>
    /// <param name="nopeus">objektin nopeus vektorina</param>
    private void AsetaNopeus(PhysicsObject pallo, Vector nopeus)
    {
        pallo.Velocity = nopeus;
    }
    

    /// <summary>
    /// Lasketaan, kuinka kauan pelaaja selviää pelissä.
    /// </summary>
    /// <returns>selviytymisaika sekuntteina</returns>
    private Timer LaskeSelviytymisAika()
    {
        Timer selviytymisAika = new Timer();
        Label piste = new Label();
        piste.TextColor = Color.Yellow;
        piste.Color = Color.Black;
        piste.BorderColor = Color.White;
        piste.Size = new Vector(olionMitta, olionMitta);
        piste.BindTo(selviytymisAika.SecondCounter);
        piste.X = Level.Right - olionMitta;
        piste.Y = Level.Top - olionMitta / 2;
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
    /// <returns>pistelaskuri, eli pelaajan sen hetkisen kerätyt pisteet</returns>
    private IntMeter LaskePisteet(double x, double y)
    {
        IntMeter pisteLaskuri = new IntMeter(0);
        Label pisteNaytto = new Label();
        pisteNaytto.Size = new Vector(olionMitta, olionMitta);
        pisteNaytto.TextColor = Color.Yellow;
        pisteNaytto.Position = new Vector(x, y);
        pisteNaytto.Color = Color.Black;
        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
        return pisteLaskuri;
    }

                    
    /// <summary>
    /// Luodaan näytölle viesti, kun pelaaja häviää ja näytetään hänen selviytymisajan
    /// suuruus ja kerättyjen pisteiden määrä
    /// </summary>
    private void NaytaGameOverViesti()
    {
        Label gameOver = new Label("Game Over \n" + "your score: " + pelaajanPisteet.Value + "\n" + "Your survival time: "
            + selviytymisaika.CurrentTime.ToString("F2") + " s \n" + "Press Enter to continue");
        gameOver.BorderColor = Color.Black;
        gameOver.Color = Color.BrightGreen;
        gameOver.TextColor = Color.Black;
        Add(gameOver);
    }
   
    
}