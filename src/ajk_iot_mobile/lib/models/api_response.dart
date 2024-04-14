class ApiResponse<T> {
  final T? data;
  final List<String> errors;
  bool get isSuccess => errors.isEmpty;

  ApiResponse({this.data, required this.errors});

  factory ApiResponse.fromJson(Map<String, dynamic> json, T Function(dynamic) fromJsonT) {
    return ApiResponse<T>(
      data: json['data'] != null ? fromJsonT(json['data']) : null,
      errors: List<String>.from(json['errors']),
    );
  }
}