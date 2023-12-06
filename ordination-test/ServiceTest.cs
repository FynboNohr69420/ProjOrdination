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
    //TC1
    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }
    //TC2
    [TestMethod]
    public void OpretDagligFast()
    {
        DateTime dateInPast = new DateTime(2023, 11, 23, 0, 0, 0);

        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());

        // Kast Exception hvis dato ligger før dags dato
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(2, lm.LaegemiddelId, 2, 2, 1, 0, dateInPast, DateTime.Now.AddDays(3)));

        // Kast Exception hvis antal er negativ
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId, 2, -4, 1, 0, DateTime.Now, DateTime.Now.AddDays(3)));

        // Kast Exception hvis patient ikke eksisterer
        Assert.ThrowsException<InvalidOperationException>(() =>
            service.OpretDagligFast(104, lm.LaegemiddelId, 2, -4, 1, 0, DateTime.Now, DateTime.Now.AddDays(3)));

        

    }

    //TC3
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