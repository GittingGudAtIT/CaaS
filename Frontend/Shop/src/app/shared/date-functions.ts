
export function localDateTimeFormat(date: Date): string{
  function twoDigit(numb: number): string{
    if(numb >= 100 || numb < 0)return '00';
    if(numb < 10) return '0'.concat(numb.toFixed());
    return numb.toFixed()
  };

  return `${
    date.getFullYear()
  }-${twoDigit(date.getMonth() + 1)
  }-${twoDigit(date.getDate())
  }T${twoDigit(date.getHours())
  }:${twoDigit(date.getMinutes())}`;
}

export function firstOfMonth(): Date{
  const date = new Date();
  return new Date(date.getFullYear(), date.getMonth(), 1);
}

export function firstOfYear(): Date{
  const date = new Date();
  return new Date(date.getFullYear(), 0, 1);
}

export function parseLocal(s : string | null): Date{
  if(!s || s.length < 16) return new Date();

  return new Date(
    Number(s.slice(0, 4)),
    Number(s.slice(5, 7)) - 1,
    Number(s.slice(8, 10)) + 1,
    Number(s.slice(11, 13)),
    Number(s.slice(14, 16))
  );
}
