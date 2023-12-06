namespace ordination_test;

using shared.Model;

[TestClass]
public class PatientTest
{
    //TC4
    [TestMethod]
    public void PatientHasName()
    {
        string cpr = "160563-1234"; // dette er et cpr nummer
        string navn = "John";
        double vægt = 83;
        
        Patient patient = new Patient(cpr, navn, vægt);
        Assert.AreEqual(navn, patient.navn);
    }

    //TC5
    [TestMethod]
    public void TestDerAltidFejler()
    {
        string cpr = "160563-1234";
        string navn = "John";
        double vægt = 83;

        Patient patient = new Patient(cpr, navn, vægt);
        Assert.AreEqual("Egon", patient.navn);
    }
}