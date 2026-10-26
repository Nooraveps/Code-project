using System;
using Jypeli;
using System.Collections.Generic;

namespace HenrinJuustoJahti;

/// @author noora
/// @version 3.4.2025
/// <summary>
/// Peli, jossa Henri hiirulainen kerää juustoja ja varoo hiirenloukkuja.
/// </summary>
public class HenrinJuustoJahti : PhysicsGame
{
    /// <summary>
    /// nopeuden vakio.
    /// </summary>
    private const double NOPEUS = 200;
    /// <summary>
    /// Hyppynopeuden vakio.
    /// </summary>
    private const double HYPPYNOPEUS = 750;
    /// <summary>
    /// Pelaaja.
    /// </summary>
    private PlatformCharacter pelaaja;
    /// <summary>
    /// Pistelaskuri.
    /// </summary>
    private IntMeter pistelaskuri;
    /// <summary>
    /// Pistenäyttö.
    /// </summary>
    private Label pistenaytto;
    /// <summary>
    /// Juustojen määrä.
    /// </summary>
    private int juustojenMaara;
    /// <summary>
    /// Lista, johon tallennetaan kerättyjen juustojen määrä
    /// </summary>
    private readonly List<int> keratytJuustot = new List<int>();
    

    /// <summary>
    /// Tämä ohjelma kutsuu aliohjelmia ja suorittaa ne.
    /// </summary>
    public override void Begin()
    {
        LuoKentta();
        LisaaNappaimet();
        LuoPistelaskuri();
    }
    
    
    /// <summary>
    /// Tämä aliohjelma luo pelikentän, sekä lisää sinne pelaajan ja tasot.
    /// </summary>
    private void LuoKentta()
    {
        Level.Background.Color = Color.Lavender;
        LisaaTaso(new Vector(0, -200), 1600, 40);
        LisaaPelaaja(new Vector(0, -180), 40, 40);
        LuoTasot();
        
        Gravity = new Vector(0, -1000);
        Camera.Follow(pelaaja);
        Camera.ZoomFactor = 1.2;
    }
    

    /// <summary>
    /// Tämä aliohjelma luo taso, johon pelaaja syntyy.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <returns></returns>
    private PhysicsObject LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.DarkBrown;
        Add(taso);
        return taso;
    } 
    

    /// <summary>
    /// Tämä aliohjelma luo satunnaisesti tasoja kutsumalla LisaaTaso aliohjelmaa.
    /// </summary>
    private void LuoTasot()
    {
        double keskipisteX = pelaaja.Position.X; 
        double y = -50; 
        Random rng = new Random();
        juustojenMaara = 0;

        for (int rivi = 0; rivi < 5; rivi++)  // Määrittää, kuinka korkealle tasoja luodaan.
        {
            for (int sarake = -2; sarake <= 2; sarake++)  
            {
                double tasoX = keskipisteX + sarake * 250 + rng.Next(-50, 50); 
                double tasoY = y + rivi * 150 + rng.Next(-20, 20); 

                PhysicsObject taso = LisaaTaso(new Vector(tasoX, tasoY), 100, 20);
                bool onHiirenloukku = rng.Next(100) < 30;
                LisaaJuustoTaiLoukku(new Vector(taso.Position.X, taso.Position.Y + 
                                        (taso.Height / 2) + 10), 20, 20, onHiirenloukku);
                if (!onHiirenloukku)
                {
                    juustojenMaara++; 
                }
            }
            
        }
        
    }
    

    /// <summary>
    /// Tämä aliohjelma lisää peliin juuston ja hiirenloukun.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <param name="onHiirenloukku"></param>
    private void LisaaJuustoTaiLoukku(Vector paikka, double leveys, double korkeus, bool onHiirenloukku = false)
    {
        PhysicsObject esine = new PhysicsObject(leveys, korkeus, Shape.Circle);
        esine.Position = paikka;

        if (onHiirenloukku)
        {
            esine.Color = Color.Red;
            esine.Tag = "hiirenloukku";
            Image hiirenloukkuKuva = LoadImage("Hiirenloukku.png");
            esine.Image = hiirenloukkuKuva;
            esine.Size *= 5;
        }
        else
        {
            esine.Color = Color.Yellow;
            esine.Tag = "juusto";
            Image juustoKuva = LoadImage("Juusto.png");
            esine.Image = juustoKuva;
            esine.Size *= 4;
        }

        Add(esine);
    }
    

    /// <summary>
    /// Tämä aliohjelma lisää pelii pelaajan eli Henri hiirulaisen.
    /// </summary>
    /// <param name="paikka"></param>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja = new PlatformCharacter(leveys, korkeus);
        pelaaja.Position = paikka;
        pelaaja.Mass = 4.0;
        Image henrinKuva = LoadImage("Henri.png");
        pelaaja.Image = henrinKuva;
        pelaaja.Size *= 1.5;
        Add(pelaaja);

        
        AddCollisionHandler(pelaaja, "juusto", TormaaJuustoon);
        AddCollisionHandler(pelaaja, "hiirenloukku", PelaajaKuolee);
    }
    

    /// <summary>
    /// Tämä aliohjelma lisää peliin näppäinohjaimet.
    /// </summary>
    private void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja, -NOPEUS);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu oikealle", pelaaja, NOPEUS);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja, HYPPYNOPEUS);
    }
    

    /// <summary>
    /// Tämä aliohjelma pistää Henrin liikkumaan.
    /// </summary>
    /// <param name="henri"></param>
    /// <param name="nopeus"></param>
    private void Liikuta(PlatformCharacter henri, double nopeus)
    {
        henri.Walk(nopeus);
    }
    

    /// <summary>
    /// Tämä aliohjelma pistää Henrin hyppäämään.
    /// </summary>
    /// <param name="henri"></param>
    /// <param name="nopeus"></param>
    private void Hyppaa(PlatformCharacter henri, double nopeus)
    {
        henri.Jump(nopeus);
    }
    

    /// <summary>
    /// Tämä aliohjelma lisää toiminnon, kun pelaaja kerää juuston. Tämän lisäksi aliohjelma lisää toiminnon
    /// sille, kun pelaaja on kerännyt kaikki juustot ja voittaa pelin.
    /// </summary>
    /// <param name="henri"></param>
    /// <param name="juusto"></param>
    private void TormaaJuustoon(PhysicsObject henri, PhysicsObject juusto)
    {
        juusto.Destroy();
        pistelaskuri.Value += 1;
        
        keratytJuustot.Add(1);

        MessageDisplay.Clear(); // Poistaa edelliset viestit
        MessageDisplay.Add("JEEEEE löysit JUUSTOA!"); 

        if (pistelaskuri.Value >= juustojenMaara)
        {
            Voitto();
        }
    }
    

    /// <summary>
    /// Tämä aliohjelma lisää toiminnon, kun pelaaja törmää hiirenloukkuun ja häviää pelin.
    /// </summary>
    /// <param name="henri"></param>
    /// <param name="hiirenloukku"></param>
    private void PelaajaKuolee(PhysicsObject henri, PhysicsObject hiirenloukku)
    {
        Label viesti = new Label("VOI EI, jäit kiinni HIIRENLOUKKUUN :(");
        viesti.TextColor = Color.White;
        viesti.Color = Color.Red;
        viesti.Font = new Font(50);
        viesti.X = 0; 
        viesti.Y = 0; 
        Add(viesti); 

        pelaaja.Destroy();
        hiirenloukku.Destroy();
        Timer.SingleShot(2, Exit);
    }
    
    
    /// <summary>
    /// Tämä aliohjelma lisää viestin kun pelaaja on kerännyt kaikki juustot.
    /// </summary>
    private void Voitto()
    {
        int juustojenSumma = keratytJuustot.Count;
        double kulunutAika = Laskekulunutaika();

        Label voittoViesti = new Label($" HYVÄ!! Löysit Henrille {juustojenSumma} JUUSTOA!\n Juustojahdissasi " +
                                       $"kesti {kulunutAika:F2} sekuntia!");
        voittoViesti.TextColor = Color.White;
        voittoViesti.Color = Color.Green;
        voittoViesti.Font = new Font(50);
        voittoViesti.Position = Vector.Zero;
        Add(voittoViesti);

        Timer.SingleShot(5.0, Exit);
    }
    
    
    /// <summary>
    /// Tämä aliohjelma näyttää kerättyjen pisteiden eli juustojen määrän.
    /// </summary>
    private void LuoPistelaskuri()
    {
        pistelaskuri = new IntMeter(0);

        pistenaytto = new Label();
        pistenaytto.Position = LaskeSijaintiRuudulla(90, 50);
        pistenaytto.TextColor = Color.Black;
        pistenaytto.Color = Color.White;
        pistenaytto.Title = "Kerätyt juustot: ";

        pistenaytto.BindTo(pistelaskuri);
        Add(pistenaytto);
    }
    

    /// <summary>
    /// Tämä aliohjelma laskee kuinka kauan peli kestää.
    /// </summary>
    /// <returns></returns>
    private double Laskekulunutaika()
    {
        double aika = Time.SinceStartOfGame.TotalSeconds;
        return aika;
    }
    
    
    /// <summary>
    /// Tämä aliohjelma laskee halutun objektin sijainnin ruudulla suhteessa reuniin.
    /// </summary>
    /// <param name="leveys"></param>
    /// <param name="korkeus"></param>
    /// <returns></returns>
    private Vector LaskeSijaintiRuudulla(double leveys, double korkeus)
    {
        double x = Screen.Left + leveys;
        double y = Screen.Top - korkeus;
        return new Vector(x, y);
    }
}