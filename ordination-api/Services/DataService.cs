using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[5];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            //Alle datoer herunder ændret fra 2021 til 2023
            ordinationer[0] = new PN(new DateTime(2023, 1, 1), new DateTime(2023, 1, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2023, 2, 12), new DateTime(2023, 2, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2023, 1, 20), new DateTime(2023, 1, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2023, 1, 1), new DateTime(2023, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2023, 1, 10), new DateTime(2023, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2023, 1, 23), new DateTime(2023, 1, 24), lm[2]);
            
            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }

    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato) {

       

        // Hent patienten / lægemidlet fra databasen baseret på id
        Patient patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId)!;
        Laegemiddel laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId)!;

        //kan også bruges??
        //Patient patient = db.Patienter.Find(patientId);
        //Laegemiddel laegemiddel = db.Laegemiddler.Find(laegemiddelId);

        // Hvis patient eller lægemiddel ikke findes, så smid en exception
        if (patient == null || laegemiddel == null)
        {
            // Håndter fejlen, f.eks. ved at returnere null eller kaste en exception
            throw new Exception("Fejl: Patient eller lægemiddel ikke fundet i databasen.");
        }
    

        PN nyPN = new PN(startDato, slutDato, antal, laegemiddel);

        patient.ordinationer.Add(nyPN);

        db.SaveChanges();

        return nyPN;
    }

    public DagligFast OpretDagligFast(int? patientId, int? laegemiddelId, 
        double antalMorgen, double antalMiddag, double antalAften, double antalNat, 
        DateTime startDato, DateTime slutDato)
    {

        if (patientId == null || laegemiddelId == null)
        {
            throw new ArgumentNullException("Enten patient eller lægemiddel findes ikke");
        }
        if (startDato < DateTime.Today)
        {
            throw new InvalidOperationException("Ordineringen kan ikke have startdato i fortiden");
        }
        if (antalAften < 0 || antalMorgen < 0 || antalMiddag < 0 || antalNat < 0)
        {
            throw new InvalidOperationException("Dosering kan ikke være mindre end 0");
        }
        if (slutDato < startDato)
        {
            throw new InvalidOperationException("Slutdato kan ikke ligge før Startdato");
        }

        Patient p = db.Patienter.Find(patientId)!;
        Laegemiddel l = db.Laegemiddler.Find(laegemiddelId)!;

        DagligFast k = new DagligFast(startDato, slutDato, l, antalMorgen, antalMiddag, antalAften, antalNat);

        p.ordinationer.Add(k);
        db.SaveChanges();

        return k;

    }


    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato) {
        // Hent patienten / lægemidlet fra databasen baseret på id
        Patient patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId)!;
        Laegemiddel laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId)!;

        // Hvis patient eller lægemiddel ikke findes, så smid en exception
        if (patient == null || laegemiddel == null)
        {
            // Håndter fejlen, f.eks. ved at returnere null eller kaste en exception
            throw new InvalidOperationException("Fejl: Patient eller lægemiddel ikke fundet i databasen.");
        }

        // Validere at startdato ikke er i fortiden
        if (startDato < DateTime.Today)
        {
            throw new InvalidOperationException("Ordineringen kan ikke have startdato i fortiden");
        }

        if (slutDato < startDato)
        {
            throw new InvalidOperationException("Slutdato kan ikke ligge før Startdato");
        }

        // Validere at doser ikke er negativ
        if (doser.Any(d => d.antal < 0))
        {
            throw new InvalidOperationException("Dosis kan ikke være negativ");
        }

        DagligSkæv nyDagligSkæv = new DagligSkæv(startDato, slutDato, laegemiddel, doser);

        patient.ordinationer.Add(nyDagligSkæv);
        db.SaveChanges();

        return nyDagligSkæv;
        
    }
    //Har været i gang med denne, men ved ikke hvad den skal udføre lol, lige nu kommer den bare med tekst output
    public string AnvendOrdination(int id, Dato dato) {

        PN ordination = db.PNs.FirstOrDefault(o => o.OrdinationId == id)!;

        if (ordination == null)
        {
            throw new Exception("Fejl: Ordination ikke fundet.");
        }

        DateTime datoDateTime = dato.dato;

        if (datoDateTime < ordination.startDen || datoDateTime > ordination.slutDen)
        {
            return "Fejl: Dato uden for gyldighedsperiode.";
        }

        ordination.dates.Add(dato);

        db.SaveChanges();

        return "Ordination anvendt succesfuldt.";
    }

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
    /// 

	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId) {

        Patient patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId)!;

        Laegemiddel laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId)!;

        double patientVægt = patient.vaegt;

        double anbefaletDosis;

        if (patientVægt < 25)
        {
            anbefaletDosis = patientVægt * laegemiddel.enhedPrKgPrDoegnLet;

        }
        else if (patientVægt >= 25 && patientVægt <= 125)
        {
            anbefaletDosis = patientVægt * laegemiddel.enhedPrKgPrDoegnNormal;

        }
        else
        {
            anbefaletDosis = patientVægt * laegemiddel.enhedPrKgPrDoegnTung;
        }

        return anbefaletDosis;
	}
    
}