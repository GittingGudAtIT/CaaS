import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductAdmin } from 'src/app/shared/dtos/product-admin';
import { ProductManagementService } from 'src/app/shared/services/product-management.service';

@Component({
  selector: 'caas-product-detail-create',
  templateUrl: './product-detail-create.component.html',
  styles: [
  ]
})
export class ProductDetailCreateComponent {
  productForm!: FormGroup;
  returnUrl: string = '/admin/products';
  submited: boolean = false;
  params: any;
  initialPrice: number = 0;


  constructor(
    private route: ActivatedRoute,
    private productService: ProductManagementService,
    private router: Router,
    private fb: FormBuilder
  ){}

  backParams(): any{
    delete this.params['returnUrl'];
    return this.params;
  }

  submit(){
    const key = sessionStorage.getItem('appKey')?? '';
    const product = new ProductAdmin(
      '', this.productForm.value.name, this.productForm.value.description,
      Number(this.productForm.value.price), this.productForm.value.downloadLink,
      Math.floor(Math.random() * 400)
    );
    this.productService.createProduct(product, key)
      .subscribe(res => {
        if(res && res['id']){
          const id = res['id'];
          this.router.navigate(['/admin/products/' + id], { queryParams: this.params});
        } else alert('something wen\'t wrong during product creation');
      });
  }

  ngOnInit(): void {

    this.productForm = this.fb.group({
      name: ['new Product', [Validators.required, Validators.maxLength(50)]],
      price: [0, [Validators.required]],
      description: ['description', [Validators.required, Validators.maxLength(255)]],
      downloadLink: ['https://www.example-link.com', [ Validators.required, Validators.maxLength(255),
        Validators.pattern('(http(s)?):\/\/(www\.)?[a-zA-Z0-9@:%._\+\-~#=]{2,256}\.[a-z]{2,6}')
      ]],
    });

    this.route.queryParams.subscribe((params) => {
      const s = params['returnUrl'];
      if(s) this.returnUrl = s;
      this.params = {...params};
    });
  }
}
