export enum MinType {
  ProductCount = 1,
  CartSum = 2
}

export const minTypeLabelMapping: Record<number, string> = {
  [MinType.ProductCount]: "Product Count",
  [MinType.CartSum]: "Cart Sum",
}

export const minTypeValues = [ MinType.ProductCount, MinType.CartSum ];
