resource "aws_dynamodb_table" "jobs" {
  name         = "smarthire-jobs"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "id"

  attribute {
    name = "id"
    type = "S"
  }

  tags = {
    Environment = "dev"
    Project     = "SmartHire"
  }
}

resource "aws_dynamodb_table" "candidates" {
  name         = "smarthire-candidates"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "candidateId"

  attribute {
    name = "candidateId"
    type = "S"
  }

  tags = {
    Environment = "dev"
    Project     = "smarthire"
  }
}

resource "aws_dynamodb_table" "applications" {
  name         = "smarthire-applications"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "applicationId"

  attribute {
    name = "applicationId"
    type = "S"
  }

  tags = {
    Environment = "dev"
    Project     = "smarthire"
  }
}

