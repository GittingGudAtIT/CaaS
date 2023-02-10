import { Discount } from "./dtos/discount";
import { MinType } from "./enums/min-type";
import { OffType } from "./enums/off-type";

export function evaluateConfigMessage(discount: Discount): string{
  var message = '';
  if(discount.minType === MinType.CartSum){
    if(discount.offType === OffType.Fixed){
      // fixed offvalue at cart sum
      message = `Save ${
        discount.offValue.toFixed(2)
      } mu. at a cart value of ${
        discount.minValue.toFixed()
      } mu.`;
    } else if(discount.offType === OffType.Percentual) {
      // percentual offvalue at curt sum
      message = `Get ${
        (discount.offValue * 100).toFixed(2)
      }% off at a cart value of ${
        discount.minValue.toFixed()
      } mu.`;
    } else if(discount.offType === OffType.None && discount.freeProducts.length){
      const singular = discount.freeProducts.length < 2;
      message = `Get free product${singular ? '' : 's'
      } when having a cart value above or equal to ${discount.minValue.toFixed(2)} mu.`;
    }
  } else if(discount.minType === MinType.ProductCount){
    // fixed off value of products
    if(discount.offType === OffType.Fixed) {
      message = `Save ${
        discount.offValue.toFixed(2)
      } mu. when buying ${
        discount.minValue.toFixed(0)
      } of the specified products`;
    // buy 3 get 1 free e.g.
    } else if(discount.offType === OffType.FreeProduct){
        message = `Buy ${discount.minValue.toFixed(0)
      } get ${discount.offValue} for free`;
    // get percentual off by buying amount
    } else if(discount.offType === OffType.Percentual){
        message = `Buy ${discount.minValue.toFixed(0)
      } to get ${(discount.offValue * 100).toFixed(2)}% off`
    } else if(discount.offType === OffType.None && discount.freeProducts.length){
      const singular = discount.freeProducts.length < 2;
      message = `Get free product${
        singular ? '' : 's'
      } when buying ${
        discount.minValue.toFixed()
      } of the specified products`;
    }
  }

  if(discount.freeProducts.length && discount.offType !== OffType.None && message !== ''){
    message += ` and get ${
      discount.freeProducts.length >= 1 ? 'these products ' : 'product '
    } * for free`;
  }

  if(message === '')
    return 'invalid configuration';

  return message;
}
