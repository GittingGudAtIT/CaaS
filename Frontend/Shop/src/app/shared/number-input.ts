
export function numberFieldKeyDown(e: KeyboardEvent, allowDecimal: boolean = false) : boolean {
  if(`0123456789${allowDecimal? '.' : ''}`.indexOf(e.key) < 0
  && [
    'ArrowLeft',
    'ArrowRight',
    'Backspace',
    'Delete',
    'Home',
    'End',
    'Tab'
  ].indexOf(e.code) < 0){
    e.preventDefault();
    return false;
  }
  return true;
}
