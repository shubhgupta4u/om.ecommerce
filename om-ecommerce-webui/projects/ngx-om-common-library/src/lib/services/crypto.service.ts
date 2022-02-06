import { Inject, Injectable, Injector } from '@angular/core';
import { AES, enc ,mode,pad} from "crypto-js";
import { Common_Setting_TOKEN } from '../../public-api';
import { CommonSetting } from '../interfaces/setting';

@Injectable({
  providedIn: 'root'
})
export class CryptoService {

  readonly cryptoKey:any;
  readonly ivKey:any;

  constructor(private injector: Injector) { 
    const setting = injector.get<CommonSetting>(Common_Setting_TOKEN);
    if(setting && setting.cryptoKey){
      this.cryptoKey = enc.Utf8.parse(setting.cryptoKey);
      this.ivKey = enc.Utf8.parse(setting.cryptoKey);
    }
    else{
      throw new Error("Failed to initialize Crypto Service. Cipher key is required parameter.");
    }
  }
  encrypt(plainText:string): string {
    let cypherText:string = AES.encrypt(enc.Utf8.parse(plainText), this.cryptoKey,{
      keySize: 128 / 8,
      iv: this.ivKey,
      mode: mode.CBC,
      padding: pad.Pkcs7
      }).toString(); 
    return cypherText;
  }

  decrypt(cypherText:string): string {
      let bytes:any  = AES.decrypt(cypherText, this.cryptoKey,{
        keySize: 128 / 8,
        iv: this.ivKey,
        mode: mode.CBC,
        padding: pad.Pkcs7
        });
      let plaintext:string = bytes.toString(enc.Utf8);
      return plaintext;
  }
}
