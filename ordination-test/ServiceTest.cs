namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }
    
    [TestMethod]
    public void OpretDagligFast()
    {
        DateTime dateInPast = new DateTime(2023, 11, 23, 0, 0, 0);

        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        
        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        //TC1: Tjekker om der bliver oprettet en ny ordination
        Assert.AreEqual(2, service.GetDagligFaste().Count());

        //TC2: Kast Exception hvis antal er negativ
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId, 2, -4, 1, 0, DateTime.Now, DateTime.Now.AddDays(3)));

        //TC3: Kast Exception hvis patient ikke eksisterer
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(104, lm.LaegemiddelId, 2, -4, 1, 0, DateTime.Now, DateTime.Now.AddDays(3)));

        //TC 4: Kast Exception hvis dato ligger før dags dato
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(2, lm.LaegemiddelId, 2, 2, 1, 0, dateInPast, DateTime.Now.AddDays(3)));

        // TC 5: Kast Exception hvis Slutdato ligger før Startdato
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(104, lm.LaegemiddelId, 2, 1, 1, 0, DateTime.Now.AddDays(3), DateTime.Now));

    }

    [TestMethod]
    public void DagligFastDoegndosis()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DagligFast p = new DagligFast(DateTime.Now, DateTime.Now.AddDays(3), lm, 2, 2, 1, 0);

        DagligFast e = new DagligFast(DateTime.Now, DateTime.Now.AddDays(3), lm, 7, 12, 3, 5);

        DagligFast f = new DagligFast(DateTime.Now, DateTime.Now.AddDays(3), lm, 16, 4, 8, 1);

        double pDoegnDosis = p.doegnDosis();

        double eDoegnDosis = e.doegnDosis();

        double fDoegnDosis = (f.doegnDosis() + 1);

        // TC 11: 2+2+1+0 = 5
        Assert.AreEqual(5, pDoegnDosis);

        // TC 12: 7+12+3+5 = 27
        Assert.AreEqual(27, eDoegnDosis);

        // TC 13: 16+4+8+1 = 29 - but result is 30
        Assert.AreNotEqual(29, fDoegnDosis);
    }

    [TestMethod]
    public void TestAtKodenSmiderEnException()
    {
        // Herunder skal man så kalde noget kode,
        // der smider en exception.

        // Hvis koden _ikke_ smider en exception,
        // så fejler testen.

        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.ThrowsException<ArgumentNullException>(() =>
            service.OpretDagligFast(null, lm.LaegemiddelId, 2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3)));

        
    }
}