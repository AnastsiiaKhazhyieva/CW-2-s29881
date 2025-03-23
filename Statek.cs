namespace Czw2;

public interface IHazardNotifier
{
    void NotifyHazard(string message);
}

public class OverfillExeption : Exception
{
    public OverfillExeption(string message) : base(message) { }
}

public class Kontener
{
    public double MasaLadunku { get; set; }
    public double Wysokosc { get; set; }
    public double WagaWlasna { get; set; }
    public double Glebokosc { get; set; }
    public string NumerSeryjny { get; private set; }
    public int MaxLadowosc { get; set; }

    private static int numer = 1;
    public Kontener(string type)
    {
        NumerSeryjny = $"KON-{type}-{numer++}";
    }

    public void Oproznienie()
    {
        MasaLadunku = 0;
    }

    public virtual void Zaladowanie(double masa)
    {
        if (MasaLadunku + masa > MaxLadowosc)
        {
            throw new OverfillExeption($"Overfill w kontenerze {NumerSeryjny}");
        }
        MasaLadunku += masa;
    }
    
    public override string ToString()
    {
        return $"{NumerSeryjny} - Masa: {MasaLadunku}/{MaxLadowosc} kg";
    }
}

public class KontenerNaPlyny : Kontener, IHazardNotifier
{
    public bool IsHazard { get; set; }

    public KontenerNaPlyny() : base("L") { }

    public override void Zaladowanie(double massa)
    {
        double MaxZaladowanie = IsHazard ? MaxLadowosc * 0.5 : MaxLadowosc * 0.9;
        if (MasaLadunku + massa > MaxZaladowanie)
        {
            NotifyHazard($"Niebezpieczna operacja w kontenerze {NumerSeryjny}");
            throw new OverfillExeption($"Overfill w kontenerze {NumerSeryjny}");
        }
        base.Zaladowanie(massa);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"!! {message}");
    }
}

public class KontenerNaGaz : Kontener, IHazardNotifier
{
    public double Cisnienie { get; set; }
    
    public KontenerNaGaz() : base("G") { }

    public override void Zaladowanie(double massa)
    {
        if (MasaLadunku + massa > MaxLadowosc)
        {
            NotifyHazard($"Kontener {NumerSeryjny} został przepełniony");
            throw new OverfillExeption($"Overfill w kontenerze {NumerSeryjny}");
        }
        base.Zaladowanie(massa);
    }

    public void NotifyHazard(string message)
    {
        Console.WriteLine($"!! {message}");
    }

    public void OproznienieGaz()
    {
        MasaLadunku *= 0.05;
    }
}

public class KontenerChlodniczy : Kontener
{
    public string TypProduktu { get; set; }
    public double Temperatura { get; set; }

    public KontenerChlodniczy() : base("C") { }

    public bool SprawdzenieTemp(double minTemperatura)
    {
        return Temperatura >= minTemperatura;
    }
}

public class Statek
{
    private List<Kontener> Kontenery { get; set; } = new List<Kontener>();
    public string Nazwa { get; set; }
    public double MaksPredkosc { get; set; }
    public int MaksLiczbKont { get; set; }
    public double MaksWagaKont { get; set; }

    public Statek(string nazwa, double maksPredkosc, int maksLiczbKont, double maksWagaKont)
    {
        Nazwa = nazwa;
        MaksPredkosc = maksPredkosc;
        MaksLiczbKont = maksLiczbKont;
        MaksWagaKont = maksWagaKont;
    }

    public void ZaladujKontener(Kontener kontener, double masa)
    {
        if (Kontenery.Count >= MaksLiczbKont)
        {
            throw new InvalidOperationException("Statek został przepełniony.");
        }
        
        double aktualnaWaga = 0;
        foreach (var k in Kontenery)
        {
            aktualnaWaga += k.MasaLadunku + k.WagaWlasna;
        }

        if (aktualnaWaga + kontener.MasaLadunku + kontener.WagaWlasna > MaksWagaKont * 1000)
        {
            throw new InvalidOperationException("Przekroczono maksymalną wagę statku.");
        }
        kontener.Zaladowanie(masa);
        Kontenery.Add(kontener);
    }

    public void ZaladujKontenery(List<Kontener> newKontenery, double masa)
    {
        foreach (var kontener in newKontenery)
        {
            ZaladujKontener(kontener, masa);
        }
    }

    public void RozladujKontener(Kontener kontener)
    {
        if (Kontenery.Contains(kontener))
        {
            kontener.Oproznienie();
        }
        else throw new InvalidOperationException("Kontener nie znajduję się na statku.");
    }

    public void UsunKontener(Kontener kontener)
    {
        if (Kontenery.Contains(kontener))
        {
            Kontenery.Remove(kontener);
        }
        else throw new InvalidOperationException("Kontener nie znajduję się na statku.");
    }
    
    public void ZastapKontener(string numerSeryjny, Kontener nowyKontener, double masa)
    {
        for (int i = 0; i < Kontenery.Count; i++)
        {
            if (Kontenery[i].NumerSeryjny == numerSeryjny)
            {
                UsunKontener(Kontenery[i]);
                ZaladujKontener(nowyKontener, masa);
                return;
            }
        }

        throw new InvalidOperationException("Kontener o podanym numerze nie istnieje na statku.");
    }

    public void PrzeniesKontener(Statek statek, Kontener kontener)
    {
        UsunKontener(kontener);
        statek.ZaladujKontener(kontener, kontener.MasaLadunku);
    }
    
    public void WyswietlInformacje()
    {
        Console.WriteLine($"Statek: {Nazwa}");
        Console.WriteLine($"Maksymalna prędkość: {MaksPredkosc} węzłów");
        Console.WriteLine($"Maksymalna liczba kontenerów: {MaksLiczbKont}");
        Console.WriteLine($"Maksymalna waga ładunku: {MaksWagaKont} ton");
        Console.WriteLine("\nZaładowane kontenery:");
        
        foreach (Kontener kontener in Kontenery)
        {
            Console.WriteLine(kontener);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Statek statek = new Statek("Statek1", 30, 100, 50);
        
        KontenerNaPlyny kontenerPlyny = new KontenerNaPlyny
        {
            WagaWlasna = 500,
            MaxLadowosc = 3000,
            IsHazard = true
        };

        KontenerNaGaz kontenerGaz = new KontenerNaGaz
        {
            WagaWlasna = 400,
            MaxLadowosc = 2500,
            Cisnienie = 10
        };

        KontenerChlodniczy kontenerChlodniczy = new KontenerChlodniczy
        {
            WagaWlasna = 600,
            MaxLadowosc = 3500,
            TypProduktu = "Ryba",
            Temperatura = 2
        };
        
        statek.ZaladujKontener(kontenerPlyny, 1200);
        statek.ZaladujKontener(kontenerChlodniczy, 1000);
        statek.ZaladujKontener(kontenerGaz, 900);
        
        
        statek.WyswietlInformacje();
        
        if (!kontenerChlodniczy.SprawdzenieTemp(0))
        {
            Console.WriteLine("\nTemperatura jest zbyt niska dla produktu!");
        }
        
        statek.RozladujKontener(kontenerPlyny);
        Console.WriteLine("\nPo rozładowaniu kontenera:");
        statek.WyswietlInformacje();
        
        statek.UsunKontener(kontenerGaz);
        Console.WriteLine("\nPo usunięciu kontenera:");
        statek.WyswietlInformacje();

        KontenerNaPlyny nowyKontenerNaPlyny = new KontenerNaPlyny()
        {
            WagaWlasna = 500,
            MaxLadowosc = 3400,
            IsHazard = false
        };
        
        statek.ZastapKontener(kontenerChlodniczy.NumerSeryjny, nowyKontenerNaPlyny, 1300);
        Console.WriteLine("\nPo zastąpieniu kontenera:");
        statek.WyswietlInformacje();
        
        Statek drugiStatek = new Statek("Statek2", 22, 40, 40);
        statek.PrzeniesKontener(drugiStatek, nowyKontenerNaPlyny);
        Console.WriteLine("\nPo przeniesieniu kontenera na drugi statek:");
        statek.WyswietlInformacje();
        drugiStatek.WyswietlInformacje();
    }
}