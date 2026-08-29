export interface Review {
  id: number;
  productId: number;
  rating: number;
  comment: string;
  createdAt: string;
  userName: string;
}

export interface CreateReviewRequest {
  productId: number;
  rating: number;
  comment: string;
}
