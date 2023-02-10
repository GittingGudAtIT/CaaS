export enum OffType{
  None = 0,
  Fixed = 1,
  Percentual = 2,
  FreeProduct = 3
}

export const offTypeLabelMapping: Record<number, string> = {
  [OffType.None]: "None",
  [OffType.Fixed]: "Fixed",
  [OffType.Percentual]: "Percentual",
  [OffType.FreeProduct]: "Free Product",
}

export const offTypeValues = [ OffType.None, OffType.Fixed, OffType.Percentual, OffType.FreeProduct ];
