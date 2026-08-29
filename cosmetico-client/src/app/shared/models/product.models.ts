export interface Product {
  id: number;
  name: string;
  price: number;
  stockQuantity: number;
  imagePath: string | null;
  categoryName: string;
  brandName: string;
  averageRating: number;
  reviewCount: number;
}

export interface IngredientRef {
  id: number;
  name: string;
  isAllergen: boolean;
}

export interface ProductDetail extends Product {
  description: string;
  createdAt: string;
  isActive: boolean;
  categoryId: number;
  brandId: number;
  ingredients: IngredientRef[];
}

export interface ProductPayload {
  name: string;
  description: string;
  price: number;
  stockQuantity: number;
  categoryId: number;
  brandId: number;
  ingredientIds: number[];
}

export interface UpdateProductPayload extends ProductPayload {
  isActive: boolean;
}

export interface ProductQuery {
  page?: number;
  pageSize?: number;
  search?: string | null;
  categoryId?: number | null;
  brandId?: number | null;
  minPrice?: number | null;
  maxPrice?: number | null;
  sortBy?: string | null;
}
