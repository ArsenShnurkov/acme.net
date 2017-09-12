namespace Oocx.Pkcs.Asn1BaseTypes
{
    public interface IAsn1Element : IAsn1Entity
    {
        byte Tag { get; }

        byte[] LengthBytes { get; }

        int Length { get; }

        int Size { get; }
    }
}